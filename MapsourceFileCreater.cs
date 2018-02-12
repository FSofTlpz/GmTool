/*
Copyright (C) 2015 Frank Stinner

This program is free software; you can redistribute it and/or modify it 
under the terms of the GNU General Public License as published by the 
Free Software Foundation; either version 3 of the License, or (at your 
option) any later version. 

This program is distributed in the hope that it will be useful, but 
WITHOUT ANY WARRANTY; without even the implied warranty of 
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General 
Public License for more details. 

You should have received a copy of the GNU General Public License along 
with this program; if not, see <http://www.gnu.org/licenses/>. 


Dieses Programm ist freie Software. Sie können es unter den Bedingungen 
der GNU General Public License, wie von der Free Software Foundation 
veröffentlicht, weitergeben und/oder modifizieren, entweder gemäß 
Version 3 der Lizenz oder (nach Ihrer Option) jeder späteren Version. 

Die Veröffentlichung dieses Programms erfolgt in der Hoffnung, daß es 
Ihnen von Nutzen sein wird, aber OHNE IRGENDEINE GARANTIE, sogar ohne 
die implizite Garantie der MARKTREIFE oder der VERWENDBARKEIT FÜR EINEN 
BESTIMMTEN ZWECK. Details finden Sie in der GNU General Public License. 

Sie sollten ein Exemplar der GNU General Public License zusammen mit 
diesem Programm erhalten haben. Falls nicht, siehe 
<http://www.gnu.org/licenses/>. 
*/
using GarminCore;
using GarminCore.DskImg;
using GarminCore.Files;
using GarminCore.Files.Typ;
using GarminCore.SimpleMapInterface;
using System;
using System.Collections.Generic;
using System.IO;
using System.Drawing;

namespace GmTool {
   class MapsourceFileCreater {

      /// <summary>
      /// Daten einer Kartenkachel
      /// </summary>
      public class TileInfo {

         /// <summary>
         /// Karten-ID aus der TRE-Datei
         /// </summary>
         public uint MapID;
         /// <summary>
         /// wird i.A. aus dem Basisnamen der TRE-Datei oder dem Namen des übergeordneten Verzeichnisses erzeugt
         /// </summary>
         public uint MapNumber;
         /// <summary>
         /// i.A. Beschreibung der Karte aus der TRE-Datei
         /// </summary>
         public string Description;

         /// <summary>
         /// Liste der Dateigrößen in Byte (zu den jeweiligen Dateinamen)
         /// </summary>
         public List<UInt32> SubFileSize;
         /// <summary>
         /// Liste der zugehörigen Dateinamen
         /// </summary>
         public List<string> SubFileName;

         /// <summary>
         /// Begrenzung in Grad
         /// </summary>
         public Longitude West;
         public Longitude East;
         public Latitude South;
         public Latitude North;

         public bool HasCopyright;


         public TileInfo() {
            Description = "";
            MapID = 0;
            MapNumber = 0;
            SubFileSize = new List<uint>();
            SubFileName = new List<string>();
            North = new Latitude(0);
            South = new Latitude(0);
            West = new Longitude(0);
            East = new Longitude(0);
            HasCopyright = false;
         }

         public TileInfo(TileInfo ti) : this() {
            Description = ti.Description;
            MapID = ti.MapID;
            MapNumber = ti.MapNumber;
            SubFileSize = new List<uint>(ti.SubFileSize);
            SubFileName = new List<string>(ti.SubFileName);
            North = new Latitude(ti.North);
            South = new Latitude(ti.South);
            West = new Longitude(ti.West);
            East = new Longitude(ti.East);
            HasCopyright = ti.HasCopyright;
         }

         public TileInfo(File_TDB.TileMap tm) : this() {
            Description = tm.Description;
            MapID = tm.Mapnumber;
            MapNumber = tm.Mapnumber;
            SubFileSize = new List<uint>(tm.DataSize);
            SubFileName = new List<string>(tm.Name);
            North = new Latitude(tm.North);
            South = new Latitude(tm.South);
            West = new Longitude(tm.West);
            East = new Longitude(tm.East);
            HasCopyright = tm.HasCopyright != 0;
         }

         public override string ToString() {
            return string.Format("MapID={0}, MapNumber={1}, {2}°..{3}°/{4}°..{5}°, {6}", MapID, MapNumber, West.ValueDegree, East.ValueDegree, South.ValueDegree, North.ValueDegree, Description);
         }

      }

      /// <summary>
      /// erzeugt einige für Mapsource wichtige Dateien
      /// </summary>
      /// <param name="infiles"></param>
      /// <param name="pid">wenn größer 0 wird diese PID verwendet</param>
      /// <param name="fid">wenn größer 0 wird diese FID verwendet</param>
      /// <param name="codepage">wenn größer 0 wird diese Codepage verwendet</param>
      /// <param name="mindimension">Mindestgröße der Umgrenzung der Linien und Flächen</param>
      /// <param name="ovpointtypes">Punkttypen für die Overviewkarte</param>
      /// <param name="ovlinetypes">Linientypen für die Overviewkarte</param>
      /// <param name="ovareatypes">Flächentypen für die Overviewkarte</param>
      /// <param name="tdbfile">Name der TDB-Datei</param>
      /// <param name="ovfile">Name der Overviewdatei</param>
      /// <param name="typfile">Name der TYP-Datei</param>
      /// <param name="mdxfile">Name der MDX-Datei</param>
      /// <param name="mdrfile">Name der MDR-Datei</param>
      /// <param name="notdbfile">wenn true, keine TDB-Datei erzeugen</param>
      /// <param name="noovfile">wenn true, keine Overviewdatei erzeugen</param>
      /// <param name="notypfile">wenn true, keine TYP-Datei erzeugen</param>
      /// <param name="nomdxfile">wenn true, keine MDX-Datei erzeugen</param>
      /// <param name="nomdrfile">wenn true, keine MDR-Datei erzeugen</param>
      /// <param name="noinstfiles">wenn true, keine Installations-Datei erzeugen</param>
      /// <param name="tdb_productversion"></param>
      /// <param name="tdb_routable"></param>
      /// <param name="tdb_familyname"></param>
      /// <param name="tdb_mapseriesname"></param>
      /// <param name="tdb_description"></param>
      /// <param name="tdb_highestroutable"></param>
      /// <param name="tdb_hasdem"></param>
      /// <param name="tdb_hasprofile"></param>
      /// <param name="tdb_maxcoordbits4overview"></param>
      /// <param name="copyrightsegments"></param>
      /// <param name="outputpath">Ausgabepfad</param>
      /// <param name="overwrite">true, wenn ev. schon vorhandene Dateien überschrieben werden können</param>
      static public void CreateFiles4Mapsource(List<string> infiles,
                                                int pid, int fid, int codepage,
                                                int mindimension,
                                                SortedSet<int> ovpointtypes, SortedSet<int> ovlinetypes, SortedSet<int> ovareatypes,
                                                string tdbfile, string ovfile, string typfile, string mdxfile, string mdrfile,
                                                bool notdbfile, bool noovfile, bool notypfile, bool nomdxfile, bool nomdrfile, bool noinstfiles,
                                                ushort tdb_productversion,
                                                short tdb_routable,
                                                string tdb_familyname,
                                                string tdb_mapseriesname,
                                                string tdb_description,
                                                short tdb_highestroutable,
                                                short tdb_hasdem,
                                                short tdb_hasprofile,
                                                short tdb_maxcoordbits4overview,
                                                List<File_TDB.SegmentedCopyright.Segment> copyrightsegments,
                                                string outputpath, bool overwrite) {
         // wenn die Daten gesetzt sind, werden sie ev. auf einen gültigen Bereich eingegrenzt
         if (pid > 0)
            pid &= 0xFFFF;
         if (fid > 0)
            fid &= 0xFFFF;
         if (codepage > 0)
            codepage &= 0xFFFF;

         // wenn die Daten NICHT gesetzt sind, werden sie auf einen internen Wert gesetzt
         if (!Directory.Exists(outputpath))
            Directory.CreateDirectory(outputpath);

         if (string.IsNullOrEmpty(ovfile))
            ovfile = "gmtool.img";
         if (!Path.IsPathRooted(typfile))
            ovfile = Path.GetFullPath(Path.Combine(outputpath, ovfile));

         if (string.IsNullOrEmpty(typfile))
            typfile = "gmtool.typ";
         if (!Path.IsPathRooted(typfile))
            typfile = Path.GetFullPath(Path.Combine(outputpath, typfile));

         if (string.IsNullOrEmpty(mdxfile))
            mdxfile = "gmtool.mdx";
         if (!Path.IsPathRooted(mdxfile))
            mdxfile = Path.GetFullPath(Path.Combine(outputpath, mdxfile));

         if (string.IsNullOrEmpty(mdrfile))
            mdrfile = "gmtool_mdr.img";
         if (!Path.IsPathRooted(mdrfile))
            mdrfile = Path.GetFullPath(Path.Combine(outputpath, mdrfile));

         if (string.IsNullOrEmpty(tdbfile))
            tdbfile = "gmtool.tdb";
         if (!Path.IsPathRooted(tdbfile))
            tdbfile = Path.GetFullPath(Path.Combine(outputpath, tdbfile));

         if (mindimension <= 0)
            mindimension = 2;

         if (infiles == null)
            infiles = new List<string>();
         if (ovpointtypes == null)
            ovpointtypes = new SortedSet<int>();
         if (ovlinetypes == null)
            ovlinetypes = new SortedSet<int>();
         if (ovareatypes == null)
            ovareatypes = new SortedSet<int>();

         // Wenn die Overview-Map schon in der Inputliste enthalten ist, wird sie dort entfernt.
         //if (!string.IsNullOrEmpty(ovfile))
         //   if (Path.GetExtension(ovfile).ToUpper() == ".IMG")
         //      if (File.Exists(ovfile)) {
         //         string tmp = Path.GetFullPath(Path.Combine(outputpath, ovfile)).ToUpper();
         //         for (int i = 0; i < infiles.Count; i++)
         //            if (infiles[i].ToUpper() == tmp) {
         //               infiles.RemoveAt(i);
         //               break;
         //            }
         //      }

         // wenn möglich/nötig, zusätzliche Infos ermitteln
         uint ovmapnumber = 0, ovmapid = 0;
         SortedSet<int> linetypes = new SortedSet<int>(),
                        areatypes = new SortedSet<int>(),
                        pointtypes = new SortedSet<int>();
         List<TileInfo> tileinfo = new List<TileInfo>();
         string tdb_overviewdescription = "";
         List<File_TDB.SegmentedCopyright.Segment> tdb_copyrightsegments = new List<File_TDB.SegmentedCopyright.Segment>();

         GetInfos(infiles,
                  ref pid, ref fid, ref codepage,
                  ref tdb_productversion,
                  ref tdb_familyname,
                  ref tdb_mapseriesname,
                  ref tdb_routable,
                  ref tdb_highestroutable,
                  ref tdb_hasprofile,
                  ref tdb_hasdem,
                  ref tdb_maxcoordbits4overview,
                  ref tdb_description,
                  ref tdb_copyrightsegments,
                  ref tdb_overviewdescription,
                  ref ovfile, ref typfile, ref mdxfile, ref mdrfile,
                  out ovmapnumber, out ovmapid,
                  pointtypes, linetypes, areatypes, tileinfo);
         //throw new Exception("Es kann keine Product- und/oder Family-ID ermittelt werden.");

         // ev. Overviewmp erzeugen
         if (!noovfile) {
            CreateOverviewmap(ovfile,
                              infiles,
                              ovmapnumber,
                              mindimension,
                              ref tdb_maxcoordbits4overview,
                              ovpointtypes,
                              ovlinetypes,
                              ovareatypes,
                              overwrite);
            // auch die Daten dieser neuen Overviewmap einsammeln
            int tmp_pid = 0, tmp_fid = 0, tmp_codepage = 0;

            ushort tmp_productversion = 0;
            string tmp_familyname = "";
            string tmp_mapseriesname = "";
            short tmp_routable = 0;
            short tmp_highestroutable = 0;
            short tmp_hasprofile = 0;
            short tmp_hasdem = 0;
            short tmp_maxcoordbits4overview = 0;
            string tmp_mapdescription = "";
            List<File_TDB.SegmentedCopyright.Segment> tmp_copyrightsegments = new List<File_TDB.SegmentedCopyright.Segment>();
            string tmp_overviewdescription = "";

            string tmp_ovfile = "", tmp_typfile = "", tmp_mdxfile = "", tmp_mdrfile = "";
            uint tmp_ovmapnumber, tmp_ovmapid;
            SortedSet<int> tmp_linetyps = new SortedSet<int>(), tmp_polygontyps = new SortedSet<int>(), tmp_pointtyps = new SortedSet<int>();
            List<TileInfo> tmp_tileinfo = new List<TileInfo>();
            GetInfos(new string[] { ovfile },
                     ref tmp_pid,
                     ref tmp_fid,
                     ref tmp_codepage,
                     ref tmp_productversion,
                     ref tmp_familyname,
                     ref tmp_mapseriesname,
                     ref tmp_routable,
                     ref tmp_highestroutable,
                     ref tmp_hasprofile,
                     ref tmp_hasdem,
                     ref tmp_maxcoordbits4overview,
                     ref tmp_mapdescription,
                     ref tmp_copyrightsegments,
                     ref tmp_overviewdescription,
                     ref tmp_ovfile, ref tmp_typfile, ref tmp_mdxfile, ref tmp_mdrfile,
                     out tmp_ovmapnumber, out tmp_ovmapid,
                     tmp_linetyps, tmp_polygontyps, tmp_pointtyps, tmp_tileinfo);
            tileinfo.Add(tmp_tileinfo[0]);
         }

         // ev. MDX-Datei erzeugen
         if (!nomdxfile)
            CreateMdxFile(mdxfile, (ushort)pid, (ushort)fid, tileinfo, overwrite);

         // ev. TDB-Datei erzeugen
         if (!notdbfile)
            CreateTdbFile(tdbfile, pid, fid, codepage,
                          string.IsNullOrEmpty(tdb_familyname) ? fid.ToString() : tdb_familyname,
                          string.IsNullOrEmpty(tdb_mapseriesname) ? "GMTOOL-Series" : tdb_mapseriesname,
                          tdb_productversion,
                          (byte)tdb_routable,
                          string.IsNullOrEmpty(tdb_description) ? "GMTOOL-Karte" : tdb_description,
                          (byte)tdb_highestroutable,
                          (byte)tdb_hasdem,
                          (byte)tdb_hasprofile,
                          tdb_maxcoordbits4overview,
                          ReBuildTDBCopyright(copyrightsegments, tdb_copyrightsegments),
                          ovmapnumber,
                          tileinfo,
                          overwrite);

         // Dummy-Typdatei erzeugen
         if (!notypfile)
            CreateTypFile(typfile, pid, fid, codepage, linetypes, areatypes, pointtypes, overwrite);

         // MDR-Datei erzeugen ?????????????

         mdrfile = "";

         // Batchdateien für Registrierung/Dereistrierung erzeugen
         if (!noinstfiles)
            CreateBatches(outputpath, fid, typfile, mdxfile, mdrfile, ovfile, tdbfile, overwrite);

      }

      /// <summary>
      /// erzeugt aus einer alten Copyright-Segment-Liste (<see cref="File_TDB.SegmentedCopyright.Segment"/>) und den gewünschten eine neue Liste
      /// </summary>
      /// <param name="optsegments"></param>
      /// <param name="orgsegments"></param>
      /// <returns></returns>
      static List<File_TDB.SegmentedCopyright.Segment> ReBuildTDBCopyright(List<File_TDB.SegmentedCopyright.Segment> optsegments, IList<File_TDB.SegmentedCopyright.Segment> orgsegments) {
         List<File_TDB.SegmentedCopyright.Segment> newsegments = new List<File_TDB.SegmentedCopyright.Segment>();

         for (int i = 0; i < Math.Max(orgsegments.Count, optsegments.Count); i++) {
            File_TDB.SegmentedCopyright.Segment segment = i < orgsegments.Count ?
                                                                     new File_TDB.SegmentedCopyright.Segment(orgsegments[i]) :
                                                                     new File_TDB.SegmentedCopyright.Segment(File_TDB.SegmentedCopyright.Segment.CopyrightCodes.CopyrightInformation,
                                                                                                             File_TDB.SegmentedCopyright.Segment.WhereCodes.ProductInformationAndPrinting,
                                                                                                             "");
            if (i >= optsegments.Count) // altes Segment unverändert übernehmen
               newsegments.Add(segment);
            else {
               if (optsegments[i].Copyright != null)  // Segment löschen
                  continue;
               else
                  segment.Copyright = optsegments[i].Copyright;

               if (optsegments[i].CopyrightCode != File_TDB.SegmentedCopyright.Segment.CopyrightCodes.Unknown)
                  segment.CopyrightCode = optsegments[i].CopyrightCode;

               if (optsegments[i].WhereCode != File_TDB.SegmentedCopyright.Segment.WhereCodes.Unknown)
                  segment.WhereCode = optsegments[i].WhereCode;

               newsegments.Add(segment);
            }
         }

         return newsegments;
      }

      #region Infos einsammeln

      /// <summary>
      /// versucht, die verschiedenen Daten zu setzen, wenn sie kleiner als 0 oder eine leere Zeichenkette sind
      /// </summary>
      /// <param name="infiles">Liste der zu berücksichtigenden Dateien</param>
      /// <param name="productid"></param>
      /// <param name="familyid"></param>
      /// <param name="codepage"></param>
      /// <param name="productversion"></param>
      /// <param name="familyname"></param>
      /// <param name="mapseriesname"></param>
      /// <param name="routable"></param>
      /// <param name="highestroutable"></param>
      /// <param name="hasprofile"></param>
      /// <param name="hasdem"></param>
      /// <param name="maxcoordbits4overview"></param>
      /// <param name="mapdescription"></param>
      /// <param name="copyrightsegments"></param>
      /// <param name="overviewdescription"></param>
      /// <param name="ovfile"></param>
      /// <param name="typfile"></param>
      /// <param name="mdxfile"></param>
      /// <param name="mdrfile"></param>
      /// <param name="ovmapnumber"></param>
      /// <param name="ovmapid"></param>
      /// <param name="pointtyps"></param>
      /// <param name="linetyps"></param>
      /// <param name="areatyps"></param>
      /// <param name="tileinfo"></param>
      /// <returns></returns>
      static bool GetInfos(IList<string> infiles,
                           ref int productid,
                           ref int familyid,
                           ref int codepage,
                           ref ushort productversion,
                           ref string familyname,
                           ref string mapseriesname,
                           ref short routable,
                           ref short highestroutable,
                           ref short hasprofile,
                           ref short hasdem,
                           ref short maxcoordbits4overview,
                           ref string mapdescription,
                           ref List<File_TDB.SegmentedCopyright.Segment> copyrightsegments,
                           ref string overviewdescription,
                           ref string ovfile,
                           ref string typfile,
                           ref string mdxfile,
                           ref string mdrfile,
                           out uint ovmapnumber,
                           out uint ovmapid,
                           SortedSet<int> pointtyps,
                           SortedSet<int> linetyps,
                           SortedSet<int> areatyps,
                           List<TileInfo> alltileinfo) {
         ovmapnumber = ovmapid = 0;
         List<TileInfo> tileinfotdb = new List<TileInfo>();
         List<TileInfo> tileinforeal = new List<TileInfo>();

         foreach (string file in infiles) {
            Console.Write("untersuche Datei " + file + " ...");

            string ext = Path.GetExtension(file).ToUpper();

            if (ext == ".MPS") {

               if (productid < 0 || familyid < 0)
                  using (BinaryReaderWriter brw = new BinaryReaderWriter(file, true)) {
                     GetInfosFromMPS(brw, ref productid, ref familyid);
                     brw.Dispose();
                  }

            } else if (ext == ".TYP") {

               if (string.IsNullOrEmpty(typfile))
                  typfile = file;
               if (productid < 0 || familyid < 0)
                  using (BinaryReaderWriter brw = new BinaryReaderWriter(file, true)) {
                     GetInfosFromTYP(brw, ref productid, ref familyid);
                     brw.Dispose();
                  }

            } else if (ext == ".LBL") {

               if (codepage < 0)
                  using (BinaryReaderWriter brw = new BinaryReaderWriter(file, true)) {
                     GetInfosFromLBL(brw, null, ref codepage);
                     brw.Dispose();
                  }

            } else if (ext == ".MDX") {

               if (string.IsNullOrEmpty(mdxfile))
                  mdxfile = file;

            } else if (ext == ".MDR") {

               if (string.IsNullOrEmpty(mdrfile))
                  mdrfile = file;

            } else if (ext == ".TRE") {

               using (BinaryReaderWriter brw = new BinaryReaderWriter(file, true)) {
                  GetInfosFromTRE(brw, null, file, pointtyps, linetyps, areatyps, tileinforeal);
                  brw.Dispose();
               }

            } else if (ext == ".GMP") {

               using (BinaryReaderWriter brw = new BinaryReaderWriter(file, true)) {
                  GetInfosFromGMP(brw, file, ref codepage, pointtyps, linetyps, areatyps, tileinforeal);
                  brw.Dispose();
               }

            } else if (ext == ".IMG") {

               using (BinaryReaderWriter br = new BinaryReaderWriter(file, true)) {
                  GetInfosFromIMG(br, ref productid, ref familyid, ref codepage, pointtyps, linetyps, areatyps, tileinforeal);
                  br.Dispose();
               }

            } else if (ext == ".TDB") {

               using (BinaryReaderWriter br = new BinaryReaderWriter(file, true)) {
                  GetInfosFromTDB(br,
                                    ref productid,
                                    ref familyid,
                                    ref productversion,
                                    ref codepage,
                                    ref familyname,
                                    ref mapseriesname,
                                    ref routable,
                                    ref highestroutable,
                                    ref hasprofile,
                                    ref hasdem,
                                    ref maxcoordbits4overview,
                                    ref mapdescription,
                                    ref copyrightsegments,
                                    ref ovmapnumber,
                                    tileinfotdb);
                  ovmapid = ovmapnumber;
                  br.Dispose();

               }

               //} else if (ext == ".RGN") {


               //TileInfo ti = tileinfo[tileinfo.Count - 1];
               //foreach (string tilefile in sf.AllFilenames4Basename(basename)) {
               //   ti.SubFileSize.Add(sf.Filesize(sf.FilenameIdx(tilefile)));
               //   ti.SubFileName.Add(tilefile);
               //}



            } else {

               Console.Write(" ignoriert");

            }
            Console.WriteLine();

            if (ovmapnumber <= 0 &&
                file.ToUpper() == ovfile.ToUpper() &&
                tileinforeal.Count > 0) {
               ovmapnumber = tileinforeal[tileinforeal.Count - 1].MapNumber;
               ovmapid = tileinforeal[tileinforeal.Count - 1].MapID;
            }

         }

         // tdbtileinfo und tileinfo zu alltileinfo integrieren; TDB-Beschreibung hat Vorrang
         for (int i = tileinfotdb.Count - 1; i >= 0; i--) {
            TileInfo titdb = tileinfotdb[i];
            bool found = false;
            for (int j = 0; j < tileinforeal.Count; j++) {
               TileInfo tireal = tileinforeal[j];
               if (tireal.MapNumber == titdb.MapNumber) {    // passendes ti gefunden
                  if (titdb.Description != "")
                     tireal.Description = titdb.Description;
                  tileinforeal.Remove(tireal);
                  tileinfotdb.Remove(titdb);
                  alltileinfo.Add(tireal);
                  found = true;
                  break;
               }
            }
            if (!found) // keine realen Infos zur TDB gefunden
               tileinfotdb.Remove(titdb);
         }

         alltileinfo.Reverse();
         alltileinfo.AddRange(tileinforeal);

         // falls keine bestehende Overview-Karte gefunden wurde:
         if (ovmapnumber == 0 ||
             ovmapid == 0) {
            foreach (TileInfo ti in tileinforeal) {
               ovmapnumber = Math.Max(ovmapnumber, ti.MapNumber);
               ovmapid = Math.Max(ovmapid, ti.MapID);
            }
            ovmapnumber++;         // als neue OverviewMapnumber wird die größte Map-ID + 1 verwendet
            ovmapid++;
         }

         // falls bis jetzt für einige Daten noch keine Entscheidung getroffen wurde:
         if (codepage < 0)
            codepage = 1252;

         return productid > 0 &&
                familyid > 0;
      }

      static void GetInfosFromTDB(BinaryReaderWriter brw,
                                  ref int productid,
                                  ref int familyid,
                                  ref ushort productversion,
                                  ref int codepage,
                                  ref string familyname,
                                  ref string seriesname,
                                  ref short routable,
                                  ref short maxroutingtype,
                                  ref short contour,
                                  ref short dem,
                                  ref short maxcoordbits4overview,
                                  ref string mapdescription,
                                  ref List<File_TDB.SegmentedCopyright.Segment> copyrightsegments,
                                  ref uint overviewmapno,
                                  List<TileInfo> tileinfo) {

         File_TDB tdb = new File_TDB();
         tdb.Read(brw);

         if (productid < 0)
            productid = tdb.Head.ProductID;
         if (familyid < 0)
            familyid = tdb.Head.FamilyID;
         if (productversion <= 0)
            productversion = tdb.Head.ProductVersion;
         if (codepage < 0)
            codepage = (int)tdb.Head.CodePage;
         if (string.IsNullOrEmpty(familyname))
            familyname = tdb.Head.MapFamilyName;
         if (string.IsNullOrEmpty(seriesname))
            seriesname = tdb.Head.MapSeriesName;
         if (routable < 0)
            routable = tdb.Head.Routable;
         if (maxroutingtype < 0)
            maxroutingtype = tdb.Head.HighestRoutable;
         if (contour < 0)
            contour = tdb.Head.HasProfileInformation;
         if (dem < 0)
            dem = tdb.Head.HasDEM;
         if (maxcoordbits4overview < 0)
            maxcoordbits4overview = tdb.Head.MaxCoordbits4Overview;
         mapdescription = tdb.Mapdescription.Text;
         copyrightsegments = tdb.Copyright.Segments;
         overviewmapno = tdb.Overviewmap.Mapnumber;

         if (tdb.Overviewmap.Mapnumber > 0) {                  // wenn vorhanden, dann das 1. Tile
            TileInfo ti = new TileInfo();
            ti.East.ValueDegree = tdb.Overviewmap.East;
            ti.North.ValueDegree = tdb.Overviewmap.North;
            ti.South.ValueDegree = tdb.Overviewmap.South;
            ti.West.ValueDegree = tdb.Overviewmap.West;
            ti.Description = tdb.Overviewmap.Description;
            ti.MapNumber = tdb.Overviewmap.Mapnumber;
            ti.MapID = tdb.Overviewmap.Mapnumber;

            tileinfo.Add(ti);
         }

         for (int i = 0; i < tdb.Tilemap.Count; i++)
            tileinfo.Add(new TileInfo(tdb.Tilemap[i]));

      }

      static void GetInfosFromMPS(BinaryReaderWriter brw, ref int productid, ref int familyid) {
         File_MPS mps = new File_MPS();
         mps.Read(brw);
         if (mps.Maps.Count > 0) {
            if (familyid < 0)
               familyid = mps.Maps[0].FamilyID;
            if (productid < 0)
               productid = mps.Maps[0].ProductID;
         }
      }

      static void GetInfosFromTYP(BinaryReaderWriter brw, ref int productid, ref int familyid) {
         StdFile_TYP typ = new StdFile_TYP();
         typ.Read(brw);
         if (familyid < 0)
            familyid = typ.FamilyID;
         if (productid < 0)
            productid = typ.ProductID;
      }

      static void GetInfosFromTRE(BinaryReaderWriter brw, StdFile_TRE tre, string trefile, SortedSet<int> pointtyps, SortedSet<int> linetyps, SortedSet<int> areatyps, List<TileInfo> tileinfo) {
         string basename = Path.GetFileNameWithoutExtension(trefile);
         string directory = "";
         if (trefile.Contains(Path.DirectorySeparatorChar.ToString()) ||
             trefile.Contains(Path.AltDirectorySeparatorChar.ToString())) {
            trefile = Path.GetFullPath(trefile);
            directory = Path.GetDirectoryName(trefile);
         }

         if (tre == null) {
            tre = new StdFile_TRE();
            tre.Read(brw);
         }

         if (tileinfo != null) {
            // TileInfo ermitteln
            TileInfo ti = new TileInfo();
            SetTileInfoDataFromTre(ti, tre);
            try {
               string name = "";
               if (directory != "")
                  name = Path.GetFileName(directory);
               for (int i = 0; i < name.Length; i++)
                  if (!char.IsDigit(name[i])) {
                     name = "";
                     break;
                  }
               if (name == "")
                  name = basename;
               ti.MapNumber = Convert.ToUInt32(name);

            } catch {
               Console.Error.WriteLine("Die MapNumber für '" + trefile + "' kann nicht ermittelt werden.");
            }
            if (directory != "")
               foreach (string tilefile in TheJob.AllFilenames4Basename(directory, basename)) {
                  FileInfo fi = new FileInfo(tilefile);
                  ti.SubFileSize.Add((uint)fi.Length);
                  ti.SubFileName.Add(Path.GetFileName(tilefile));
               }
            tileinfo.Add(ti);
         }

         // alle registrierten Objekttypen einsammeln
         SortedSet<int> tmp_lines;
         SortedSet<int> tmp_polygones;
         SortedSet<int> tmp_points;
         tre.GetAllOverviewTypes(out tmp_lines, out tmp_polygones, out tmp_points);
         foreach (int typ in tmp_lines)
            if (!linetyps.Contains(typ))
               linetyps.Add(typ);
         foreach (int typ in tmp_polygones)
            if (!areatyps.Contains(typ))
               areatyps.Add(typ);
         foreach (int typ in tmp_points)
            if (!pointtyps.Contains(typ))
               pointtyps.Add(typ);
      }

      static void GetInfosFromLBL(BinaryReaderWriter brwlbl, StdFile_LBL lbl, ref int codepage) {
         if (codepage < 0) {
            if (lbl == null) {
               lbl = new StdFile_LBL();
               lbl.Read(brwlbl, true);
            }
            codepage = lbl.Codepage;
         }
      }

      static void GetInfosFromGMP(BinaryReaderWriter brw4gmp, string gmpfile,
                                  ref int codepage,
                                  SortedSet<int> pointtyps, SortedSet<int> linetyps, SortedSet<int> areatyps, List<TileInfo> tileinfo) {
         string basename = Path.GetFileNameWithoutExtension(gmpfile);

         StdFile_GMP gmp = new StdFile_GMP();
         gmp.Read(brw4gmp);

         if (gmp.LBL != null)
            GetInfosFromLBL(brw4gmp, gmp.LBL, ref codepage);

         if (gmp.TRE != null) {
            GetInfosFromTRE(brw4gmp, gmp.TRE, basename + ".TRE", pointtyps, linetyps, areatyps, tileinfo);
            TileInfo ti = tileinfo[tileinfo.Count - 1];
            ti.SubFileSize.Add((uint)brw4gmp.Length);    // nur die GMP-Datei
            ti.SubFileName.Add(Path.GetFileName(gmpfile));
         }

      }

      static void GetInfosFromIMG(BinaryReaderWriter br4img, ref int productid, ref int familyid,
                                  ref int codepage,
                                  SortedSet<int> pointtyps, SortedSet<int> linetyps, SortedSet<int> areatyps, List<TileInfo> tileinfo) {
         SimpleFilesystem sf = new SimpleFilesystem();
         sf.Read(br4img);

         for (int i = 0; i < sf.FileCount; i++) {
            string file = sf.Filename(i);
            string ext = Path.GetExtension(file).ToUpper();
            string basename = Path.GetFileNameWithoutExtension(file).ToUpper();

            if (ext == ".MPS") {

               using (BinaryReaderWriter brw = sf.GetBinaryReaderWriter4File(file)) {
                  GetInfosFromMPS(brw, ref productid, ref familyid);
                  brw.Dispose();
               }

            } else if (ext == ".TYP") {

               using (BinaryReaderWriter brw = sf.GetBinaryReaderWriter4File(file)) {
                  GetInfosFromTYP(brw, ref productid, ref familyid);
                  brw.Dispose();
               }

            } else if (ext == ".TRE") {

               using (BinaryReaderWriter brw = sf.GetBinaryReaderWriter4File(file)) {
                  GetInfosFromTRE(brw, null, basename + ".TRE", pointtyps, linetyps, areatyps, tileinfo);
                  brw.Dispose();
               }
               TileInfo ti = tileinfo[tileinfo.Count - 1];
               foreach (string tilefile in sf.AllFilenames4Basename(basename)) {
                  ti.SubFileSize.Add(sf.Filesize(sf.FilenameIdx(tilefile)));
                  ti.SubFileName.Add(tilefile);
               }
               if (string.IsNullOrEmpty(ti.Description))
                  ti.Description = sf.ImgHeader.Description;

            } else if (ext == ".LBL") {

               using (BinaryReaderWriter brw = sf.GetBinaryReaderWriter4File(file)) {
                  GetInfosFromLBL(brw, null, ref codepage);
                  brw.Dispose();
               }

            } else if (ext == ".GMP") {

               using (BinaryReaderWriter brwgmp = sf.GetBinaryReaderWriter4File(file)) {
                  GetInfosFromGMP(brwgmp, file, ref codepage, pointtyps, linetyps, areatyps, tileinfo);
                  brwgmp.Dispose();
               }

            }
         }
      }

      #endregion

      #region Tile-Infos holen

      /// <summary>
      /// holt Infos zur Kachel aus der zugehörigen TRE-Datei
      /// </summary>
      /// <param name="ti"></param>
      /// <param name="br"></param>
      static void SetTileInfoDataFromTre(TileInfo ti, BinaryReaderWriter br) {
         StdFile_TRE tre = new StdFile_TRE();
         br.Seek(0);
         tre.Read(br);
         SetTileInfoDataFromTre(ti, tre);
      }

      /// <summary>
      /// holt Infos zur Kachel aus der zugehörigen TRE-Datei
      /// </summary>
      /// <param name="ti"></param>
      /// <param name="tre"></param>
      static void SetTileInfoDataFromTre(TileInfo ti, StdFile_TRE tre) {
         ti.MapID = tre.MapID;
         ti.West = tre.West;
         ti.East = tre.East;
         ti.South = tre.South;
         ti.North = tre.North;
         if (tre.MapDescriptionList.Count > 0)
            ti.Description = tre.MapDescriptionList[0];
         ti.HasCopyright = tre.CopyrightOffsetsList.Count > 0;
      }

      #endregion


      /// <summary>
      /// fügt eine <see cref="SimpleTileMap"/> in eine <see cref="DetailMap"/> ein
      /// <para>Dabei werden nur die definierten Punkt-, Linien- und Flächentypen verwendet. Außerdem mus die größere Seite der Objekt-<see cref="Bound"/> 
      /// bei der Bitanzahl größer als ein vorgegebener Wert sein.</para>
      /// </summary>
      /// <param name="samplemap"></param>
      /// <param name="newsm"></param>
      /// <param name="sourcebound"></param>
      /// <param name="sourcemapid"></param>
      /// <param name="mindimension"></param>
      /// <param name="maxbits4overview"></param>
      /// <param name="layer"></param>
      /// <param name="pointtypes"></param>
      /// <param name="linetypes"></param>
      /// <param name="areatypes"></param>
      static void MergeMap(DetailMap samplemap, SimpleTileMap newsm,
                           List<Bound> sourcebound, List<int> sourcemapid,
                           int mindimension,
                           ref short maxbits4overview, ref int layer,
                           SortedSet<int> pointtypes, SortedSet<int> linetypes, SortedSet<int> areatypes) {
         sourcebound.Add(newsm.MapBounds);
         sourcemapid.Add((int)newsm.MapID);
         if (layer == 0) // den Layer der 1. Kachel übernehmen
            layer = newsm.MapLayer;

         if (maxbits4overview == 0)
            maxbits4overview = (byte)(newsm.SymbolicScaleDenominatorAndBitsLevel.Bits(0) - 1);

         DetailMap newmap = newsm.BuildMapFromLevel(pointtypes, linetypes, areatypes);

         //samplemap.Merge(newsm.BuildMapFromLevel(pointtypes, linetypes, areatypes));
         foreach (var item in newmap.LineList) {
            List<MapUnitPoint> testpoly = item.GetMapUnitPoints();
            for (int p = 0; p < testpoly.Count; p++) { // alle Punkte in Rawunits bezüglich der verwendeten Bitanzahl umrechnen
               testpoly[p].X = testpoly[p].LatitudeRawUnits(maxbits4overview);
               testpoly[p].Y = testpoly[p].LongitudeRawUnits(maxbits4overview);
            }

            // unnötige Punkte entfernen
            bool removed = false;
            for (int p = 1; p < testpoly.Count; p++) {
               if (testpoly[p].X == testpoly[p - 1].X &&
                   testpoly[p].Y == testpoly[p - 1].Y) {
                  testpoly.RemoveAt(p);
                  item.RemovePoint(p, false);
                  p--;
                  removed = true;
               }
            }
            if (removed)
               item.CalculateBound();
         }

         // unnötige Linien entfernen
         for (int i = newmap.LineList.Count - 1; i >= 0; i--)
            if (newmap.LineList[i].PointCount <= 1 ||
                Coord.MapUnits2RawUnits(Math.Max(newmap.LineList[i].Bound.Width, newmap.LineList[i].Bound.Height), maxbits4overview) < mindimension)
               newmap.LineList.RemoveAt(i);

         foreach (var item in newmap.AreaList) {
            List<MapUnitPoint> testpoly = item.GetMapUnitPoints();
            for (int p = 0; p < testpoly.Count; p++) { // alle Punkte in Rawunits bezüglich der verwendeten Bitanzahl umrechnen
               testpoly[p].X = testpoly[p].LatitudeRawUnits(maxbits4overview);
               testpoly[p].Y = testpoly[p].LongitudeRawUnits(maxbits4overview);
            }

            // unnötige Punkte entfernen
            bool removed = false;
            for (int p = 1; p < testpoly.Count; p++) {
               if (testpoly[p].X == testpoly[p - 1].X &&
                   testpoly[p].Y == testpoly[p - 1].Y) {
                  testpoly.RemoveAt(p);
                  item.RemovePoint(p);
                  p--;
                  removed = true;
               }
            }
            if (removed)
               item.CalculateBound();
         }

         // unnötige Flächen entfernen
         for (int i = newmap.AreaList.Count - 1; i >= 0; i--)
            if (newmap.AreaList[i].PointCount <= 1 ||
                Coord.MapUnits2RawUnits(Math.Max(newmap.AreaList[i].Bound.Width, newmap.AreaList[i].Bound.Height), maxbits4overview) < mindimension)
               newmap.AreaList.RemoveAt(i);

         // alle Objekte übernehmen
         samplemap.PointList.AddRange(newmap.PointList);
         samplemap.LineList.AddRange(newmap.LineList);
         samplemap.AreaList.AddRange(newmap.AreaList);

         if (samplemap.DesiredBounds != null)
            samplemap.DesiredBounds.Embed(newmap.DesiredBounds);
         else
            samplemap.DesiredBounds = new Bound(newmap.DesiredBounds);

         Console.WriteLine("   " + newmap.PointList.Count.ToString() + " Punkte, " + newmap.LineList.Count.ToString() + " Linien und " + newmap.AreaList.Count.ToString() + " Flächen übernommen");
      }

      /// <summary>
      /// erzeugt eine Overviewkarte
      /// </summary>
      /// <param name="overviewimgorpath">Pfad und Name einer IMG-Datei oder ein bestehender Pfad für die Einzeldateien</param>
      /// <param name="infiles">Liste der Eingabedateien</param>
      /// <param name="mapid">ID der der Overviewmap</param>
      /// <param name="mindimension">Mindestgröße der Umgrenzung der Linien und Flächen</param>
      /// <param name="maxbits4overview">liefert die max. Bitanzahl für die Overviewkarte</param>
      /// <param name="pointtypes">Liste der gewünschten Punkt-Typen</param>
      /// <param name="linetypes">Liste der gewünschten Linien-Typen</param>
      /// <param name="areatypes">Liste der gewünschten Flächen-Typen</param>
      /// <param name="overwrite">wenn true, wird eine schon bestehende IMG-Datei überschrieben</param>
      /// <returns>Name der IMG- oder TRE-Datei wenn erfolgreich</returns>
      public static string CreateOverviewmap(string overviewimgorpath, List<string> infiles, uint mapid, int mindimension, ref short maxbits4overview,
                                             SortedSet<int> pointtypes, SortedSet<int> linetypes, SortedSet<int> areatypes,
                                             bool overwrite) {
         if (string.IsNullOrEmpty(overviewimgorpath)) {
            Console.WriteLine("Die Overview-Datei kann nicht ohne Angabe eines Pfad-Namens erzeugt werden.");
            return null;
         }
         if (infiles == null) {
            Console.WriteLine("Die Overview-Datei kann nicht ohne (notfalls leere) Liste der Eingabedateien erzeugt werden.");
            return null;
         }
         if (mapid <= 0) {
            Console.WriteLine("Die Overview-Datei kann nicht ohne Angabe einer Map-ID erzeugt werden.");
            return null;
         }
         if (mindimension <= 0) {
            Console.WriteLine("Die Overview-Datei kann nicht ohne Angabe einer Mindestgröße für Linien und Flächen erzeugt werden.");
            return null;
         }
         if (pointtypes == null || linetypes == null || areatypes == null) {
            Console.WriteLine("Die Overview-Datei kann nicht ohne (notfalls leere) Objekttyp-Listen erzeugt werden.");
            return null;
         }

         DetailMap samplemap = new DetailMap(null, new Bound()); // leere Gesamt-Detailmap zum Sammeln der Daten
         samplemap.DesiredBounds = null;

         List<Bound> sourcebound = new List<Bound>();
         List<int> sourcemapid = new List<int>();
         int layer = 0;
         maxbits4overview = 0;

         for (int i = 0; i < infiles.Count; i++) {
            string extension = Path.GetExtension(infiles[i]).ToUpper();

            if (extension == ".IMG") {
               SimpleFilesystem sf = new SimpleFilesystem();
               using (BinaryReaderWriter br = new BinaryReaderWriter(infiles[i], true)) {
                  sf.Read(br);
                  List<string> basename = sf.AllBasenames(); // i.A. nur 1 Name, d.h. die IMG-Datei enthält nur 1 Kachel
                  for (int j = 0; j < basename.Count; j++) {
                     SimpleTileMap sm = new SimpleTileMap();
                     Console.Write("werte Kachel '" + basename[j] + "' aus ... ");
                     sm.ReadFilter4Pointtypes = pointtypes;
                     sm.ReadFilter4Linetypes = linetypes;
                     sm.ReadFilter4Areatypes = areatypes;
                     sm.Read(sf, basename[j]); // Kacheldaten einlesen
                     Console.WriteLine(sf.ImgHeader.Description + ": " + string.Join("; ", sm.MapDescription));
                     MergeMap(samplemap, sm, sourcebound, sourcemapid, mindimension, ref maxbits4overview, ref layer, pointtypes, linetypes, areatypes);
                  }
                  br.Dispose();
               }
            } else if (extension == ".TRE") {
               SimpleTileMap sm = new SimpleTileMap();
               Console.Write("werte Kachel '" + infiles[i].Substring(0, infiles[i].Length - 4) + "' aus ... ");
               sm.Read(infiles[i].Substring(0, infiles[i].Length - 4));
               Console.WriteLine(string.Join("; ", sm.MapDescription));
               MergeMap(samplemap, sm, sourcebound, sourcemapid, mindimension, ref maxbits4overview, ref layer, pointtypes, linetypes, areatypes);
            }
         }
         Console.WriteLine(samplemap.PointList.Count.ToString() + " Punkte, " + samplemap.LineList.Count.ToString() + " Linien und " + samplemap.AreaList.Count.ToString() + " Flächen gesammelt");

         Console.Write("bilde Overview mit " + maxbits4overview.ToString() + " Bits ... ");
         //sourcebound = new List<Bound>() {
         //   new Bound(585728 , 606208, 2142208 , 2170880),
         //   new Bound(585728 , 606208, 2170880 , 2185216),
         //   new Bound(509952 , 548864, 2142208 , 2156544),
         //   new Bound(548864 , 585728, 2142208 , 2156544),
         //};
         //sourcemapid = new List<int>() { 70210001, 70210002, 70210003, 70210004 };
         //layer = 25;
         //maxbits4overview = 17;

         if (sourcemapid.Count > 0) {
            // Es muss je Kartenkachel, also je MapID/Bound, jeweils 1 0x4A-Polygon erzeugt werden.
            // Das Label des Polygon ist min. "\u001d" + MapID der zugehörigen Kartenkachel.
            DetailMap.Poly poly;
            Bound poly4B = null;
            for (int i = 0; i < sourcemapid.Count; i++) {
               poly = new DetailMap.Poly(0x04A00, false, true); // area that shows the area covered by a detailed map
               poly.AddPoint(sourcebound[i].Left, sourcebound[i].Top);
               poly.AddPoint(sourcebound[i].Left, sourcebound[i].Bottom);
               poly.AddPoint(sourcebound[i].Right, sourcebound[i].Bottom);
               poly.AddPoint(sourcebound[i].Right, sourcebound[i].Top);
               poly.Label = "GMTOOL" + "\u001d" + sourcemapid[i].ToString(); // unbedingt nötig für Overview-Map

               samplemap.AreaList.Add(poly);

               if (poly4B == null)
                  poly4B = new Bound(poly.Bound);
               else
                  poly4B.Embed(poly.Bound);
            }

            poly = new DetailMap.Poly(0x04B00, false, true); // area that shows the area covered by a detailed map
            poly.AddPoint(poly4B.Left, poly4B.Top);
            poly.AddPoint(poly4B.Left, poly4B.Bottom);
            poly.AddPoint(poly4B.Right, poly4B.Bottom);
            poly.AddPoint(poly4B.Right, poly4B.Top);
            samplemap.AreaList.Add(poly);

            samplemap.DesiredBounds = samplemap.CalculateBounds();
            if (!areatypes.Contains(0x04A00))
               areatypes.Add(0x04A00);
            if (!areatypes.Contains(0x04B00))
               areatypes.Add(0x04B00);

            SimpleTileMap ovm = new SimpleTileMap(); // leere Karte erzeugen
            ovm.MapDescription.Add("Overview created by gmtool");
            ovm.Copyright.Add("created by gmtool");
            ovm.MapID = mapid;
            ovm.CreationDate = DateTime.Now;
            ovm.MapLayer = layer;
            ovm.MapBounds = new Bound(samplemap.DesiredBounds);

            ovm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(1, maxbits4overview - 1);
            ovm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(0, maxbits4overview);


            ovm.SetMap(samplemap,
                        new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        pointtypes
                        },
                        new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        linetypes
                        },
                        new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        areatypes
                        });


            // OverviewMap speichern
            if (Directory.Exists(overviewimgorpath)) {      // bestehender Pfad --> als Subdateien in diesem Pfad speichern
               string basefilename = TheJob.GetBasefilename4Number((int)mapid, true);
               ovm.Write(Path.Combine(overviewimgorpath, basefilename), true);

#if DEBUG
               string filename = Path.Combine(overviewimgorpath, basefilename);
               Test_Newfile(filename + ".TRE");
               Test_Newfile(filename + ".LBL");
               Test_Newfile(filename + ".RGN");
#endif

               return Path.Combine(overviewimgorpath, basefilename + ".TRE");
            } else {
               if (Path.GetExtension(overviewimgorpath).ToUpper() == ".IMG") {
                  if (File.Exists(overviewimgorpath) && overwrite)
                     File.Delete(overviewimgorpath);
                  ovm.Write(overviewimgorpath, mapid.ToString("d8"), false);

#if DEBUG
                  Test_Newfile(overviewimgorpath);
#endif

                  return overviewimgorpath;
               }
            }
         }

         return "";
      }

      /// <summary>
      /// erzeugt eine MDX-Datei
      /// </summary>
      /// <param name="mdxfile">Name der MDX-Datei</param>
      /// <param name="productid">PID</param>
      /// <param name="familyid">FID</param>
      /// <param name="tileinfo">Liste der <see cref="TileInfo"/></param>
      /// <param name="overwrite">wenn true, wird eine ev. schon vorhande Datei überschrieben</param>
      public static void CreateMdxFile(string mdxfile, ushort productid, ushort familyid, List<TileInfo> tileinfo, bool overwrite) {
         if (string.IsNullOrEmpty(mdxfile)) {
            Console.WriteLine("Die MDX-Datei kann nicht ohne Angabe eines Namens erzeugt werden.");
            return;
         }
         if (productid <= 0) {
            Console.WriteLine("Die MDX-Datei kann nicht ohne Angabe einer Product-ID erzeugt werden.");
            return;
         }
         if (familyid <= 0) {
            Console.WriteLine("Die MDX-Datei kann nicht ohne Angabe einer Family-ID erzeugt werden.");
            return;
         }
         if (tileinfo == null || tileinfo.Count == 0) {
            Console.WriteLine("Die MDX-Datei kann nicht ohne Karten-Infos erzeugt werden.");
            return;
         }

         Console.WriteLine("erzeuge die Datei '" + mdxfile + "' ...");

         File_MDX mdx = new File_MDX();
         foreach (TileInfo ti in tileinfo) {
            File_MDX.MapEntry mapentry = new File_MDX.MapEntry(ti.MapID, productid, familyid, ti.MapNumber);
            Console.WriteLine("   " + mapentry.ToString());
            mdx.Maps.Add(mapentry);
         }

         if (!File.Exists(mdxfile) ||
             overwrite)
            using (BinaryReaderWriter bw = new BinaryReaderWriter(File.Create(mdxfile))) {
               mdx.Write(bw);
               bw.Dispose();
            }

#if DEBUG
         Test_Newfile(mdxfile);
#endif
      }

      /// <summary>
      /// erzeugt eine TDB-Datei
      /// </summary>
      /// <param name="tdbfile">Name der TDB-Datei</param>
      /// <param name="productid">PID</param>
      /// <param name="familyid">FID</param>
      /// <param name="codepage">Codepage</param>
      /// <param name="familyname"></param>
      /// <param name="seriesname">(notfalls analog familyname)</param>
      /// <param name="productversion"></param>
      /// <param name="routable"></param>
      /// <param name="description"></param>
      /// <param name="highestroutable"></param>
      /// <param name="hasdem"></param>
      /// <param name="hasprofile"></param>
      /// <param name="maxbits4overview">max. Bitanzahl für die Overviewmap</param>
      /// <param name="copyrightsegments"></param>
      /// <param name="overviewmapnumber">Karten-ID der Overviewmap (notfalls die größte vorhandene ID)</param>
      /// <param name="tileinfo">Infos je Kartenkachel und für die Overviewmap</param>
      /// <param name="overwrite">wenn true, wird eine ev. schon vorhande Datei überschrieben</param>
      public static void CreateTdbFile(string tdbfile,
                                       int productid,
                                       int familyid,
                                       int codepage,
                                       string familyname, string seriesname,
                                       ushort productversion,
                                       byte routable,
                                       string description,
                                       byte highestroutable,
                                       byte hasdem,
                                       byte hasprofile,
                                       int maxbits4overview,
                                       IList<File_TDB.SegmentedCopyright.Segment> copyrightsegments,
                                       uint overviewmapnumber,
                                       List<TileInfo> tileinfo,
                                       bool overwrite) {
         if (string.IsNullOrEmpty(tdbfile)) {
            Console.WriteLine("Die TDB-Datei kann nicht ohne Angabe eines Namens erzeugt werden.");
            return;
         }
         if (productid <= 0) {
            Console.WriteLine("Die TDB-Datei kann nicht ohne Angabe einer Product-ID erzeugt werden.");
            return;
         }
         if (familyid <= 0) {
            Console.WriteLine("Die TDB-Datei kann nicht ohne Angabe einer Family-ID erzeugt werden.");
            return;
         }
         if (codepage <= 0) {
            Console.WriteLine("Die TDB-Datei kann nicht ohne Angabe einer Codepage erzeugt werden.");
            return;
         }
         if (string.IsNullOrEmpty(familyname)) {
            Console.WriteLine("Die TDB-Datei kann nicht ohne Angabe eines Family-Namens erzeugt werden.");
            return;
         }
         if (tileinfo == null || tileinfo.Count == 0) {
            Console.WriteLine("Die TDB-Datei kann nicht ohne Karten-Infos erzeugt werden.");
            return;
         }

         if (string.IsNullOrEmpty(seriesname))
            seriesname = familyname;
         if (overviewmapnumber == 0) {
            foreach (TileInfo ti in tileinfo) {
               overviewmapnumber = Math.Max(overviewmapnumber, ti.MapNumber);
            }
         }

         Console.WriteLine("erzeuge die Datei '" + tdbfile + "' ...");

         File_TDB tdb = new File_TDB();

         tdb.Head.CodePage = (uint)codepage;
         tdb.Head.FamilyID = (ushort)familyid;
         tdb.Head.ProductID = (ushort)productid;
         tdb.Head.ProductVersion = productversion;
         tdb.Head.MapFamilyName = familyname;
         tdb.Head.MapSeriesName = seriesname;
         tdb.Head.MaxCoordbits4Overview = (byte)maxbits4overview;
         tdb.Head.Routable = routable;
         tdb.Head.HighestRoutable = highestroutable;
         tdb.Head.HasProfileInformation = hasprofile;
         tdb.Head.HasDEM = hasdem;
         tdb.Mapdescription = new File_TDB.Description(new File_TDB.BlockHeader(File_TDB.BlockHeader.Typ.Description));
         tdb.Mapdescription.Text = description;
         if (copyrightsegments.Count == 0)
            tdb.Copyright.Segments.Add(new File_TDB.SegmentedCopyright.Segment(File_TDB.SegmentedCopyright.Segment.CopyrightCodes.CopyrightInformation,
                                                                               File_TDB.SegmentedCopyright.Segment.WhereCodes.ProductInformationAndPrinting,
                                                                               "created by gmtool"));
         else
            for (int i = 0; i < copyrightsegments.Count; i++)
               tdb.Copyright.Segments.Add(new File_TDB.SegmentedCopyright.Segment(copyrightsegments[i]));

         Console.WriteLine("CodePage " + tdb.Head.CodePage.ToString());
         Console.WriteLine("FamilyID " + tdb.Head.FamilyID.ToString());
         Console.WriteLine("ProductID " + tdb.Head.ProductID.ToString());
         Console.WriteLine("ProductVersion " + tdb.Head.ProductVersion.ToString());
         Console.WriteLine("MapFamilyName " + tdb.Head.MapFamilyName);
         Console.WriteLine("MapSeriesName " + tdb.Head.MapSeriesName);
         Console.WriteLine("Mapdescription " + tdb.Mapdescription);
         Console.WriteLine("HasDEM " + tdb.Head.HasDEM.ToString());
         Console.WriteLine("HasProfileInformation " + tdb.Head.HasProfileInformation.ToString());
         Console.WriteLine("LowestMapLevel " + tdb.Head.MaxCoordbits4Overview.ToString());
         Console.WriteLine("Routable " + tdb.Head.Routable.ToString());
         Console.WriteLine("HighestRoutable " + tdb.Head.HighestRoutable.ToString());
         Console.WriteLine("Copyright " + tdb.Copyright.Segments[0].ToString());
         for (int i = 1; i < tdb.Copyright.Segments.Count; i++)
            Console.WriteLine("          " + tdb.Copyright.Segments[i].ToString());

         foreach (TileInfo ti in tileinfo) {
            if (ti.MapID != overviewmapnumber) {
               File_TDB.TileMap tilemap = new File_TDB.TileMap(new File_TDB.BlockHeader(File_TDB.BlockHeader.Typ.Tilemap));
               for (int i = 0; i < ti.SubFileName.Count; i++) {
                  tilemap.Name.Add(ti.SubFileName[i]);
                  tilemap.DataSize.Add(ti.SubFileSize[i]);
               }
               tilemap.Mapnumber = ti.MapID;
               tilemap.ParentMapnumber = overviewmapnumber;
               tilemap.West = ti.West.ValueDegree;
               tilemap.East = ti.East.ValueDegree;
               tilemap.South = ti.South.ValueDegree;
               tilemap.North = ti.North.ValueDegree;
               tilemap.HasCopyright = (byte)(ti.HasCopyright ? 1 : 0);
               tilemap.Description = ti.Description;

               Console.WriteLine("Detailkarte " + tilemap.ToString());

               tdb.Tilemap.Add(tilemap);
            } else {
               tdb.Overviewmap = new File_TDB.OverviewMap(new File_TDB.BlockHeader(File_TDB.BlockHeader.Typ.Overviewmap));
               tdb.Overviewmap.Mapnumber = ti.MapID;
               tdb.Overviewmap.ParentMapnumber = 0;
               tdb.Overviewmap.West = ti.West.ValueDegree;
               tdb.Overviewmap.East = ti.East.ValueDegree;
               tdb.Overviewmap.South = ti.South.ValueDegree;
               tdb.Overviewmap.North = ti.North.ValueDegree;
               tdb.Overviewmap.Description = ti.Description;
            }
         }

         if (!File.Exists(tdbfile) ||
             overwrite)
            using (BinaryReaderWriter bw = new BinaryReaderWriter(File.Create(tdbfile))) {
               tdb.Write(bw);
               bw.Dispose();
            }

#if DEBUG
         //Test_Newfile(tdbfile);
#endif
      }

      /// <summary>
      /// erzeugt eine TYP-Datei
      /// </summary>
      /// <param name="typfile">Dateiname</param>
      /// <param name="productid">PID</param>
      /// <param name="familyid">FID</param>
      /// <param name="codepage">Codepage</param>
      /// <param name="linetyps">Liste der Linientypen</param>
      /// <param name="areatyps">Liste der Flächentypen</param>
      /// <param name="pointtyps">Liste der Punkttypen</param>
      /// <param name="overwrite">wenn true, wird eine ev. schon vorhande Datei überschrieben</param>
      public static void CreateTypFile(string typfile, int productid, int familyid, int codepage, SortedSet<int> linetyps, SortedSet<int> areatyps, SortedSet<int> pointtyps, bool overwrite) {
         if (string.IsNullOrEmpty(typfile)) {
            Console.WriteLine("Die TYP-Datei kann nicht ohne Angabe eines Namens erzeugt werden.");
            return;
         }
         if (productid <= 0) {
            Console.WriteLine("Die TYP-Datei kann nicht ohne Angabe einer Product-ID erzeugt werden.");
            return;
         }
         if (familyid <= 0) {
            Console.WriteLine("Die TYP-Datei kann nicht ohne Angabe einer Family-ID erzeugt werden.");
            return;
         }
         if (codepage <= 0) {
            Console.WriteLine("Die TYP-Datei kann nicht ohne Angabe einer Codepage erzeugt werden.");
            return;
         }

         Console.WriteLine("erzeuge die Datei '" + typfile + "' ...");
         if (File.Exists(typfile) && !overwrite)
            throw new Exception("Die Datei '" + typfile + "' existiert schon.");

         using (BinaryReaderWriter bw = new BinaryReaderWriter(typfile, true, true, true, null)) {
            Random r = new Random();

            StdFile_TYP typ = new StdFile_TYP();
            typ.Codepage = (ushort)codepage;
            typ.FamilyID = (ushort)familyid;
            typ.ProductID = (ushort)productid;
            Console.WriteLine(string.Format("Codepage {0}, FamilyID {1}, ProductID {2}", typ.Codepage, typ.FamilyID, typ.ProductID));

            Console.WriteLine(string.Format("{0} Punkttypen", pointtyps.Count));
            foreach (int v in pointtyps) {
               uint t = (uint)(v < 0x100 ? v : v >> 8);
               uint st = (uint)(v < 0x100 ? 0 : v & 0xFF);
               POI poi = new POI(t, st);
               poi.SetText(new MultiText(new Text("0x" + v.ToString("X"))));
               poi.SetBitmaps(GetSolidBitmap(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)), 5), null, false);
               typ.Insert(poi);
            }

            Console.WriteLine(string.Format("{0} Linientypen", linetyps.Count));
            foreach (int v in linetyps) {
               uint t = (uint)(v < 0x100 ? v : v >> 8);
               uint st = (uint)(v < 0x100 ? 0 : v & 0xFF);
               Polyline line = new Polyline(t, st);
               line.SetText(new MultiText(new Text("0x" + v.ToString("X"))));
               line.Polylinetype = Polyline.PolylineType.NoBorder_Day1;
               line.DayColor1 = Color.FromArgb(r.Next(256), r.Next(256), r.Next(256));
               typ.Insert(line);
            }

            Console.WriteLine(string.Format("{0} Gebietstypen", areatyps.Count));
            foreach (int v in areatyps) {
               uint t = (uint)(v < 0x100 ? v : v >> 8);
               uint st = (uint)(v < 0x100 ? 0 : v & 0xFF);
               if (t != 0x4a && t != 0x4b) { // nicht für Hintergrundtypen
                  Polygone polygone = new Polygone(t, st);
                  polygone.SetText(new MultiText(new Text("0x" + v.ToString("X"))));
                  polygone.SetSolidColor(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)));
                  //polygone.SetBitmaps(Polygone.PolygonType.BM_Day1, GetSolidBitmap(Color.FromArgb(r.Next(256), r.Next(256), r.Next(256)), 32), null);
                  typ.Insert(polygone);
               }
            }

            typ.Write(bw);
            bw.Dispose();
         }
#if DEBUG
         Test_Newfile(typfile);
#endif
      }

      static Bitmap GetSolidBitmap(Color col, int width, int height = -1) {
         Random r = new Random();
         Bitmap bm = new Bitmap(width, height >= 0 ? height : width);
         Graphics canvas = Graphics.FromImage(bm);
         canvas.FillRectangle(new SolidBrush(col), new Rectangle(0, 0, bm.Width, bm.Height));
         return bm;
      }

      /// <summary>
      /// erzeugt die Batchdateien install.cmd und uninstall.cmd
      /// </summary>
      /// <param name="outputpath">Pfad, in dem die Batchdateien erzeugt werden</param>
      /// <param name="familyid">i.A. 4stellige Nummer</param>
      /// <param name="typfile">TYP-Datei</param>
      /// <param name="mdxfile">z.B. osmmap.mdx</param>
      /// <param name="mdrfile">z.B. osmmap_mdr.img</param>
      /// <param name="ovfile">Overviewdatei, z.B.: osmmap.img</param>
      /// <param name="tdbfile">z.B. osmmap.tdb</param>
      /// <param name="overwrite">wenn true, wird eine ev. schon vorhande Datei überschrieben</param>
      public static void CreateBatches(string outputpath, int familyid, string typfile, string mdxfile, string mdrfile, string ovfile, string tdbfile, bool overwrite) {
         if (string.IsNullOrEmpty(outputpath)) {
            Console.WriteLine("Die Installations-Batchdateien können nicht ohne Angabe eines Ausgabepfades erzeugt werden.");
            return;
         }
         if (familyid <= 0) {
            Console.WriteLine("Die Installations-Batchdateien können nicht ohne Angabe einer Family-ID erzeugt werden.");
            return;
         }
         if (string.IsNullOrEmpty(ovfile)) {
            Console.WriteLine("Die Installations-Batchdateien können nicht ohne Angabe einer Overview-Datei erzeugt werden.");
            return;
         }
         if (string.IsNullOrEmpty(tdbfile)) {
            Console.WriteLine("Die Installations-Batchdateien können nicht ohne Angabe einer TDB-Datei erzeugt werden.");
            return;
         }

         string FamilyKey = string.Format("%KEY%\\Families\\FAMILY_{0}", familyid);

         string file = Path.Combine(outputpath, "install.cmd");
         Console.WriteLine("erzeuge die Datei '" + file + "' ...");

         if (File.Exists(file) && !overwrite)
            throw new Exception("Die Datei '" + file + "' existiert schon.");
         using (BinaryReaderWriter bw = new BinaryReaderWriter(file, false, true, true, System.Text.Encoding.ASCII)) {
            //set KEY=HKLM\SOFTWARE\Wow6432Node\Garmin\MapSource
            //if %PROCESSOR_ARCHITECTURE% == AMD64 goto key_ok
            //set KEY=HKLM\SOFTWARE\Garmin\MapSource
            //:key_ok
            //reg ADD %KEY%\Families\FAMILY_7006 /v ID /t REG_BINARY /d 5e1b /f
            //reg ADD %KEY%\Families\FAMILY_7006 /v TYP /t REG_SZ /d "%~dp0fsoft3.TYP" /f
            //reg ADD %KEY%\Families\FAMILY_7006 /v IDX /t REG_SZ /d "%~dp0osmmap.mdx" /f
            //reg ADD %KEY%\Families\FAMILY_7006 /v MDR /t REG_SZ /d "%~dp0osmmap_mdr.img" /f
            //reg ADD %KEY%\Families\FAMILY_7006\1 /v Loc /t REG_SZ /d "%~dp0\" /f
            //reg ADD %KEY%\Families\FAMILY_7006\1 /v Bmap /t REG_SZ /d "%~dp0osmmap.img" /f
            //reg ADD %KEY%\Families\FAMILY_7006\1 /v Tdb /t REG_SZ /d "%~dp0osmmap.tdb" /f
            bw.WriteString("set KEY=HKLM\\SOFTWARE\\Wow6432Node\\Garmin\\MapSource\r\n", null, false);
            bw.WriteString("if %PROCESSOR_ARCHITECTURE% == AMD64 goto key_ok\r\n", null, false);
            bw.WriteString("set KEY=HKLM\\SOFTWARE\\Garmin\\MapSource\r\n", null, false);
            bw.WriteString(":key_ok\r\n", null, false);

            bw.WriteString("reg DELETE " + FamilyKey + " /f\r\n", null, false);
            int familyidswap = ((familyid & 0xFF) << 8) | ((familyid & 0xFF00) >> 8); // höher- und niederwertiges Byte austauschen
            bw.WriteString("reg ADD " + FamilyKey + " /v ID /t REG_BINARY /d " + familyidswap.ToString("x") + " /f\r\n", null, false);
            if (string.IsNullOrEmpty(typfile))
               bw.WriteString("@REM ");
            bw.WriteString("reg ADD " + FamilyKey + " /v TYP /t REG_SZ /d \"%~dp0" + Path.GetFileName(typfile) + "\" /f\r\n", null, false);
            if (string.IsNullOrEmpty(mdxfile))
               bw.WriteString("@REM ");
            bw.WriteString("reg ADD " + FamilyKey + " /v IDX /t REG_SZ /d \"%~dp0" + Path.GetFileName(mdxfile) + "\" /f\r\n", null, false);
            if (string.IsNullOrEmpty(mdrfile))
               bw.WriteString("@REM ", null, false);
            bw.WriteString("reg ADD " + FamilyKey + " /v MDR /t REG_SZ /d \"%~dp0" + Path.GetFileName(mdrfile) + "\" /f\r\n", null, false);

            FamilyKey += "\\1";
            bw.WriteString("reg ADD " + FamilyKey + " /v Loc /t REG_SZ /d \"%~dp0\\\" /f\r\n", null, false);
            if (string.IsNullOrEmpty(ovfile))
               bw.WriteString("@REM ");
            bw.WriteString("reg ADD " + FamilyKey + " /v Bmap /t REG_SZ /d \"%~dp0" + Path.GetFileName(ovfile) + "\" /f\r\n", null, false);
            if (string.IsNullOrEmpty(tdbfile))
               bw.WriteString("@REM ");
            bw.WriteString("reg ADD " + FamilyKey + " /v Tdb /t REG_SZ /d \"%~dp0" + Path.GetFileName(tdbfile) + "\" /f\r\n", null, false);
            bw.Dispose();
         }

         file = Path.Combine(outputpath, "uninstall.cmd");
         Console.WriteLine("erzeuge die Datei '" + file + "' ...");

         if (File.Exists(file) && !overwrite)
            throw new Exception("Die Datei '" + file + "' existiert schon.");
         using (BinaryReaderWriter bw = new BinaryReaderWriter(file, false, true, true, System.Text.Encoding.ASCII)) {
            //set KEY=HKLM\SOFTWARE\Wow6432Node\Garmin\MapSource
            //if %PROCESSOR_ARCHITECTURE% == AMD64 goto key_ok
            //set KEY=HKLM\SOFTWARE\Garmin\MapSource
            //:key_ok
            //reg DELETE %KEY%\Families\FAMILY_7006 /f
            bw.WriteString("set KEY=HKLM\\SOFTWARE\\Wow6432Node\\Garmin\\MapSource\r\n", null, false);
            bw.WriteString("if %PROCESSOR_ARCHITECTURE% == AMD64 goto key_ok\r\n", null, false);
            bw.WriteString("set KEY=HKLM\\SOFTWARE\\Garmin\\MapSource\r\n", null, false);
            bw.WriteString(":key_ok\r\n", null, false);
            bw.WriteString("reg DELETE " + FamilyKey + " /f\r\n", null, false);
            bw.Dispose();
         }
      }

      /// <summary>
      /// erzeugt i.W. die Dateiliste der TDB neu
      /// </summary>
      /// <param name="tdbfile"></param>
      public static void RefreshTDB(string tdbfile) {
         File.Copy(tdbfile, tdbfile + "~", true);

         int productid = -1;
         int familyid = -1;
         ushort productversion = 0;
         int codepage = -1;
         string familyname = "";
         string seriesname = "";
         short routable = -1;
         short maxroutingtype = -1;
         short hasprofile = -1;
         short hasdem = -1;
         short maxcoordbits4overview = -1;
         string mapdescription = "";
         List<File_TDB.SegmentedCopyright.Segment> copyrightsegments = new List<File_TDB.SegmentedCopyright.Segment>();
         uint overviewmapno = 0;
         List<TileInfo> tileinfo = new List<TileInfo>();

         using (BinaryReaderWriter br = new BinaryReaderWriter(tdbfile, true)) {
            GetInfosFromTDB(br,
                     ref productid,
                     ref familyid,
                     ref productversion,
                     ref codepage,
                     ref familyname,
                     ref seriesname,
                     ref routable,
                     ref maxroutingtype,
                     ref hasprofile,
                     ref hasdem,
                     ref maxcoordbits4overview,
                     ref mapdescription,
                     ref copyrightsegments,
                     ref overviewmapno,
                     tileinfo);
            br.Dispose();
         }

         bool bDEMExist = false;

         // Akt. der TileInfos
         List<TileInfo> newtileinfo = new List<TileInfo>();
         string[] trefiles = Directory.GetFiles(Path.GetDirectoryName(tdbfile), "*.TRE", SearchOption.AllDirectories);
         if (trefiles.Length > 0) { // gmapi
            foreach (string trefile in trefiles) {
               TileInfo ti = new TileInfo();
               using (BinaryReaderWriter br = new BinaryReaderWriter(trefile, true)) {
                  SetTileInfoDataFromTre(ti, br);
                  string[] mapfiles = Directory.GetFiles(Path.GetDirectoryName(trefile), Path.GetFileNameWithoutExtension(trefile) + ".*", SearchOption.TopDirectoryOnly);
                  foreach (string file in mapfiles) {
                     ti.SubFileName.Add(Path.GetFileName(file));
                     ti.SubFileSize.Add((uint)(new FileInfo(file).Length));
                     if (!bDEMExist)
                        bDEMExist = Path.GetExtension(file).ToUpper() == ".DEM";
                  }
                  newtileinfo.Add(ti);
               }
            }
         } else {
            string[] imgfiles = Directory.GetFiles(Path.GetDirectoryName(tdbfile), "*.IMG", SearchOption.TopDirectoryOnly); // MKGMAP-Standard
            foreach (string imgfile in imgfiles) {
               TileInfo ti = new TileInfo();
               using (BinaryReaderWriter br = new BinaryReaderWriter(imgfile, true)) {
                  SimpleFilesystem sf = new SimpleFilesystem();
                  sf.Read(br);
                  string basename = "";
                  for (int i = 0; i < sf.FileCount; i++) {
                     string file = sf.Filename(i);
                     if (Path.GetExtension(file) == ".TRE") {
                        basename = Path.GetFileNameWithoutExtension(file);
                        break;
                     }
                  }
                  if (basename != "") {
                     for (int i = 0; i < sf.FileCount; i++) {
                        string file = sf.Filename(i);
                        if (basename == Path.GetFileNameWithoutExtension(file).ToUpper()) {
                           ti.SubFileName.Add(file);
                           ti.SubFileSize.Add(sf.Filesize(i));
                           if (!bDEMExist)
                              bDEMExist = Path.GetExtension(file).ToUpper() == ".DEM";
                        }
                     }
                     newtileinfo.Add(ti);
                  }
               }
            }
         }

         hasdem = (short)(bDEMExist ? 1 : 0);

         if (newtileinfo.Count > 0) {
            // Beschreibungen der alten Liste übernehmen (nach Möglichkeit)
            for (int i = 0; i < newtileinfo.Count; i++) {
               newtileinfo[i].Description = newtileinfo[i].ToString();
               for (int j = 0; j < tileinfo.Count; j++) {
                  if (tileinfo[j].MapID == newtileinfo[i].MapID) {
                     newtileinfo[i].Description = tileinfo[i].Description;
                     break;
                  }
               }
            }

            CreateTdbFile(tdbfile,
                          productid,
                          familyid,
                          codepage,
                          familyname,
                          seriesname,
                          productversion,
                          (byte)routable,
                          mapdescription,
                          (byte)maxroutingtype,
                          (byte)hasdem,
                          (byte)hasprofile,
                          maxcoordbits4overview,
                          copyrightsegments,
                          overviewmapno,
                          newtileinfo,
                          true);
         }
      }

#if DEBUG

      static void Test_Newfile(string filename) {
         Console.WriteLine("=============== " + filename);
         try {
            Info4File.Info(filename, 99);
         } catch (Exception ex) {
            Console.WriteLine("FEHLER: " + ex.Message);
         }
         Console.WriteLine("===============");
      }

#endif

      #region Tests

      public static void Test1(string imgorpath, uint mapid, int layer) {
         try {
            SimpleTileMap stm = new SimpleTileMap(); // leere Karte erzeugen
            stm.MapDescription.Add("Map data (c) OpenStreetMap and its contributors\nhttp://www.openstreetmap.org/copyright\n" +
                                    "\n" +
                                    "This map data is made available under the Open Database License:\n" +
                                    "http://opendatacommons.org/licenses/odbl/1.0/\n" +
                                    "Any rights in individual contents of the database are licensed under the\n" +
                                    "Database Contents License: http://opendatacommons.org/licenses/dbcl/1.0/\n" +
                                    "\n" +
                                    "Map created with mkgmap-r3676\n" +
                                    "Program released under the GPL\n");
            stm.Copyright.Add("PROGRAM LICENCED UNDER GPL");
            stm.Copyright.Add("V2 OPENSTREETMAP.ORG CONTRIBUTORS. SEE: HTTP://WIKI.OPENSTREETMAP.ORG/INDEX.PHP/ATTRIBUTION");
            stm.MapID = mapid;
            stm.CreationDate = DateTime.Now;
            stm.MapLayer = layer;


            // Testdaten erzeugen
            DetailMap testmap = new DetailMap(null, new Bound(12.0, 14.0, 51.0, 52.0));

            testmap.PointList.Add(new DetailMap.Point(0x01212, 12.6, 51.3));

            DetailMap.Poly poly;

            poly = new DetailMap.Poly(0x00100, false, false);
            poly.AddPoint(12.0, 51.0);
            poly.AddPoint(14.0, 52.0);
            testmap.LineList.Add(poly);

            poly = new DetailMap.Poly(0x00400, false, false);
            poly.AddPoint(14.0, 51.0);
            poly.AddPoint(12.0, 52.0);
            testmap.LineList.Add(poly);

            poly = new DetailMap.Poly(0x00400, false, false);
            poly.AddPoint(12.0, 51.75);
            poly.AddPoint(13.3, 51.3);
            testmap.LineList.Add(poly);

            poly = new DetailMap.Poly(0x00600, false, false);
            poly.AddPoint(14.0, 51.5);
            poly.AddPoint(12.0, 51.5);
            testmap.LineList.Add(poly);

            poly = new DetailMap.Poly(0x11305, false, false);
            poly.AddPoint(12.2, 51.25);
            poly.AddPoint(13.8, 51.45);
            testmap.LineList.Add(poly);


            poly = new DetailMap.Poly(0x01900, false, true);
            poly.AddPoint(12.4, 51.8);
            poly.AddPoint(13.6, 51.8);
            poly.AddPoint(13.4, 51.2);
            poly.AddPoint(12.6, 51.2);
            testmap.AreaList.Add(poly);

            poly = new DetailMap.Poly(0x1101a, false, true);
            poly.AddPoint(12.8, 51.6);
            poly.AddPoint(13.3, 51.55);
            poly.AddPoint(13.2, 51.35);
            poly.AddPoint(12.9, 51.4);
            testmap.AreaList.Add(poly);


            // Level erzeugen, Objekttypen je Level definieren und die Baumstruktur der Subdivs erzeugen

            stm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(4, 17);
            stm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(3, 18);
            stm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(2, 20);
            stm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(1, 23);
            stm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(0, 24);

            stm.SetMap(testmap,
                        new List<SortedSet<int>>() {
                           new SortedSet<int>(new int[] { }),
                           new SortedSet<int>(new int[] { }),
                           new SortedSet<int>(new int[] { 0x01212 }),
                           new SortedSet<int>(new int[] { 0x01212 }),
                           new SortedSet<int>(new int[] { 0x01212 }),
                        },
                        new List<SortedSet<int>>() {
                           new SortedSet<int>(new int[] { }),
                           new SortedSet<int>(new int[] { 0x00100 }),
                           new SortedSet<int>(new int[] { 0x00100, 0x00400 }),
                           new SortedSet<int>(new int[] { 0x00100, 0x00400, 0x00600 }),
                           new SortedSet<int>(new int[] { 0x00100, 0x00400, 0x00600, 0x11305 }),
                        },
                        new List<SortedSet<int>>() {
                           new SortedSet<int>(new int[] { }),
                           new SortedSet<int>(new int[] { 0x01900 }),
                           new SortedSet<int>(new int[] { 0x01900 }),
                           new SortedSet<int>(new int[] { 0x01900, 0x1101a }),
                        });


            // speichern
            if (Directory.Exists(imgorpath)) {      // bestehender Pfad --> als Subdateien in diesem Pfad speichern
               string basefilename = TheJob.GetBasefilename4Number((int)mapid, true);
               stm.Write(Path.Combine(imgorpath, basefilename), true);

#if DEBUG
               string filename = Path.Combine(imgorpath, basefilename);
               Test_Newfile(filename + ".TRE");
               Test_Newfile(filename + ".LBL");
               Test_Newfile(filename + ".RGN");
#endif
            } else {
               if (Path.GetExtension(imgorpath).ToUpper() == ".IMG") {
                  if (File.Exists(imgorpath))
                     File.Delete(imgorpath);
                  stm.Write(imgorpath, mapid.ToString("d8"), false);
#if DEBUG
                  Test_Newfile(imgorpath);
#endif
               }
            }

         } catch (Exception ex) {
            Console.Error.WriteLine("Fehler: " + ex.Message);
         }
      }

      // Test2("../MiniMap/ms7999/osmmap.img", 79990000, 25)
      public static string Test2(string overviewimgorpath, uint mapid, int layer) {
         SimpleTileMap ovm = new SimpleTileMap(); // leere Karte erzeugen
         ovm.MapDescription.Add("Map data (c) OpenStreetMap and its contributors\nhttp://www.openstreetmap.org/copyright\n" +
                                 "\n" +
                                 "This map data is made available under the Open Database License:\n" +
                                 "http://opendatacommons.org/licenses/odbl/1.0/\n" +
                                 "Any rights in individual contents of the database are licensed under the\n" +
                                 "Database Contents License: http://opendatacommons.org/licenses/dbcl/1.0/\n" +
                                 "\n" +
                                 "Map created with mkgmap-r3676\n" +
                                 "Program released under the GPL\n");
         ovm.Copyright.Add("PROGRAM LICENCED UNDER GPL");
         ovm.Copyright.Add("V2 OPENSTREETMAP.ORG CONTRIBUTORS. SEE: HTTP://WIKI.OPENSTREETMAP.ORG/INDEX.PHP/ATTRIBUTION");
         ovm.MapID = mapid;
         ovm.CreationDate = DateTime.Now;
         ovm.MapLayer = layer;

         Bound bound = new Bound(12.0, 14.0, 51.0, 52.0);
         DetailMap map = new DetailMap(null, bound);

         // Die Ebene mit dem größten Maßstab bleibt komplett leer.
         // Alle anderen Ebenen müssen wenigstens ein 0x4A-Polygon mit dem passenden Namen enthalten.
         // Jedes 0x4A-Polygon deckt genau 1 Kartenkachel ab. Das Label des Polygon ist min. "\u001d" + MapID der zugehörigen Kartenkachel.

         DetailMap.Poly poly;

         poly = new DetailMap.Poly(0x04A00, false, true); // area that shows the area covered by a detailed map
         poly.AddPoint(bound.Left, bound.Top);
         poly.AddPoint(bound.Left, bound.Bottom);
         poly.AddPoint(bound.Right, bound.Bottom);
         poly.AddPoint(bound.Right, bound.Top);
         poly.Label = "bel. Text" + "\u001d" + (mapid + 1).ToString(); // unbedingt nötig für Overview-Map

         map.AreaList.Add(poly);

         poly = new DetailMap.Poly(0x00100, false, false);
         poly.AddPoint(12.0, 51.7);
         poly.AddPoint(14.0, 51.3);

         map.LineList.Add(poly);


         ovm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(1, 16);
         ovm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(0, 17);

         ovm.SetMap(map,
                     new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        new SortedSet<int>(new int[] { })
                     },
                     new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        new SortedSet<int>(new int[] { 0x00100 })
                     },
                     new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        new SortedSet<int>(new int[] { 0x04A00 })
                     });


         // OverviewMap speichern
         if (Directory.Exists(overviewimgorpath)) {      // bestehender Pfad --> als Subdateien in diesem Pfad speichern
            string basefilename = TheJob.GetBasefilename4Number((int)mapid, true);
            ovm.Write(Path.Combine(overviewimgorpath, basefilename), true);

#if DEBUG
            string filename = Path.Combine(overviewimgorpath, basefilename);
            Test_Newfile(filename + ".TRE");
            Test_Newfile(filename + ".LBL");
            Test_Newfile(filename + ".RGN");
#endif

            return Path.Combine(overviewimgorpath, basefilename + ".TRE");
         } else {
            if (Path.GetExtension(overviewimgorpath).ToUpper() == ".IMG") {
               if (File.Exists(overviewimgorpath))
                  File.Delete(overviewimgorpath);
               ovm.Write(overviewimgorpath, mapid.ToString("d8"), false);

#if DEBUG
               Test_Newfile(overviewimgorpath);
#endif

               return overviewimgorpath;
            }
         }

         return "";
      }

      // Test2("../MiniMap/ms7999/osmmap.img", 79990000, 25)
      public static string Test3(string overviewimgorpath, uint ovmmapid, int layer) {

         Bound bound = new Bound(12.0, 14.0, 51.0, 52.0);
         DetailMap map = new DetailMap(null, bound);

         DetailMap.Poly poly;

         poly = new DetailMap.Poly(0x00100, false, false);
         poly.AddPoint(12.0, 51.7);
         poly.AddPoint(14.0, 51.3);

         map.LineList.Add(poly);

         return Test3b("../MiniMap/ms7999/osmmap.img", map, 79990000, new int[] { 79990001 }, new Bound[] { bound }, 17, 25);
      }

      // Test2("../MiniMap/ms7999/osmmap.img", 79990000, 25)
      public static string Test3b(string overviewimgorpath, DetailMap map, uint ovmmapid, IList<int> sourcemapid, IList<Bound> sourcebound, int bits4ovm, int layer) {
         SimpleTileMap ovm = new SimpleTileMap(); // leere Karte erzeugen
         ovm.MapDescription.Add("Map data (c) OpenStreetMap and its contributors\nhttp://www.openstreetmap.org/copyright\n" +
                                 "\n" +
                                 "This map data is made available under the Open Database License:\n" +
                                 "http://opendatacommons.org/licenses/odbl/1.0/\n" +
                                 "Any rights in individual contents of the database are licensed under the\n" +
                                 "Database Contents License: http://opendatacommons.org/licenses/dbcl/1.0/\n" +
                                 "\n" +
                                 "Map created with mkgmap-r3676\n" +
                                 "Program released under the GPL\n");
         ovm.Copyright.Add("PROGRAM LICENCED UNDER GPL");
         ovm.Copyright.Add("V2 OPENSTREETMAP.ORG CONTRIBUTORS. SEE: HTTP://WIKI.OPENSTREETMAP.ORG/INDEX.PHP/ATTRIBUTION");
         ovm.MapID = ovmmapid;
         ovm.CreationDate = DateTime.Now;
         ovm.MapLayer = layer;

         Bound bound = new Bound(12.0, 14.0, 51.0, 52.0);

         // Es muss je Kartenkachel, also je MapID/Bound, jeweils 1 0x4A-Polygon.
         // Das Label des Polygon ist min. "\u001d" + MapID der zugehörigen Kartenkachel.
         DetailMap.Poly poly;
         for (int i = 0; i < sourcemapid.Count && i < sourcebound.Count; i++) {
            poly = new DetailMap.Poly(0x04A00, false, true); // area that shows the area covered by a detailed map
            poly.AddPoint(sourcebound[i].Left, sourcebound[i].Top);
            poly.AddPoint(sourcebound[i].Left, sourcebound[i].Bottom);
            poly.AddPoint(sourcebound[i].Right, sourcebound[i].Bottom);
            poly.AddPoint(sourcebound[i].Right, sourcebound[i].Top);
            poly.Label = "GMTOOL" + "\u001d" + sourcemapid[i].ToString(); // unbedingt nötig für Overview-Map

            map.AreaList.Add(poly);
         }

         ovm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(1, bits4ovm - 1);
         ovm.SymbolicScaleDenominatorAndBitsLevel.AddLevel(0, bits4ovm);

         SortedSet<int> pointtypes = new SortedSet<int>(map.GetPointTypes(false));
         pointtypes.UnionWith(map.GetPointTypes(true));
         SortedSet<int> linetypes = new SortedSet<int>(map.GetLineTypes(false));
         linetypes.UnionWith(map.GetLineTypes(true));
         SortedSet<int> areatypes = new SortedSet<int>(map.GetAreaTypes(false));
         areatypes.UnionWith(map.GetAreaTypes(true));

         ovm.SetMap(map,
                     new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        pointtypes
                     },
                     new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        linetypes
                     },
                     new List<SortedSet<int>>() {
                        new SortedSet<int>(new int[] { }),
                        areatypes
                     });


         // OverviewMap speichern
         if (Directory.Exists(overviewimgorpath)) {      // bestehender Pfad --> als Subdateien in diesem Pfad speichern
            string basefilename = TheJob.GetBasefilename4Number((int)ovmmapid, true);
            ovm.Write(Path.Combine(overviewimgorpath, basefilename), true);

#if DEBUG
            string filename = Path.Combine(overviewimgorpath, basefilename);
            Test_Newfile(filename + ".TRE");
            Test_Newfile(filename + ".LBL");
            Test_Newfile(filename + ".RGN");
#endif

            return Path.Combine(overviewimgorpath, basefilename + ".TRE");
         } else {
            if (Path.GetExtension(overviewimgorpath).ToUpper() == ".IMG") {
               if (File.Exists(overviewimgorpath))
                  File.Delete(overviewimgorpath);
               ovm.Write(overviewimgorpath, ovmmapid.ToString("d8"), false);

#if DEBUG
               Test_Newfile(overviewimgorpath);
#endif

               return overviewimgorpath;
            }
         }

         return "";
      }

      #endregion

   }
}
