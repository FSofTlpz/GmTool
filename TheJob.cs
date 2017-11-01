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
using GarminCore.SimpleMapInterface;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace GmTool {
   class TheJob {

      Options opt;

      List<string> InputFiles;
      string Output;
      char[] wildcards = new char[] { '?', '*' };


      public TheJob() { }

      public void Run(Options opt) {
         this.opt = opt;

         try {
            if (!PrepareInputData(opt.InputWithSubdirs))
               return;

            switch (opt.ToDo) {
               case Options.ToDoType.Nothing: // dann aber ev. Dateieigenschaften ändern
                  PrepareOutputData();
                  foreach (string file in InputFiles)
                     ChangeProperties(file, Output, opt.OutputOverwrite, opt);
                  break;

               case Options.ToDoType.Info:
               case Options.ToDoType.LongInfo:
               case Options.ToDoType.VeryLongInfo:
                  foreach (string file in InputFiles)
                     Info4File.Info(file, opt.ToDo == Options.ToDoType.Info ? 0 :
                                          opt.ToDo == Options.ToDoType.LongInfo ? 1 : 2);
                  break;

               case Options.ToDoType.Split:
               case Options.ToDoType.SplitRecursive:
               case Options.ToDoType.SplitJoin:
               case Options.ToDoType.SplitRecursiveJoin:
                  if (PrepareOutputData())
                     foreach (string file in InputFiles)
                        switch (opt.ToDo) {
                           case Options.ToDoType.Split: Split(file, Output, opt.OutputOverwrite, false, false); break;
                           case Options.ToDoType.SplitRecursive: Split(file, Output, opt.OutputOverwrite, true, false); break;
                           case Options.ToDoType.SplitJoin: Split(file, Output, opt.OutputOverwrite, false, true); break;
                           case Options.ToDoType.SplitRecursiveJoin: Split(file, Output, opt.OutputOverwrite, true, true); break;
                        }

                  break;

               case Options.ToDoType.Join:
               case Options.ToDoType.JoinDevice:
               case Options.ToDoType.JoinTile:
                  if (PrepareOutputData())
                     Join(InputFiles,
                          Output,
                          opt.Description.IsSet ? opt.Description.ValueAsString : "Beschreibung ?",
                          opt.OutputOverwrite,
                          opt.ToDo == Options.ToDoType.JoinDevice, opt.ToDo == Options.ToDoType.JoinTile);
                  break;

               case Options.ToDoType.CreateFiles4Mapsource:
                  if (PrepareOutputData()) {
                     List<File_TDB.SegmentedCopyright.Segment.CopyrightCodes> CopyrightCodes = new List<File_TDB.SegmentedCopyright.Segment.CopyrightCodes>();
                     List<File_TDB.SegmentedCopyright.Segment.WhereCodes> CopyrightWhereCodes = new List<File_TDB.SegmentedCopyright.Segment.WhereCodes>();
                     List<string> CopyrightText = new List<string>();

                     MapsourceFileCreater.CreateFiles4Mapsource(InputFiles,
                                                               opt.PID.IsSet ? (int)opt.PID.ValueAsUInt : -1,
                                                               opt.FID.IsSet ? (int)opt.FID.ValueAsUInt : -1,
                                                               opt.Codepage.IsSet ? (int)opt.Codepage.ValueAsUInt : -1,
                                                               opt.MapsourceMinDimension.IsSet ? (int)opt.MapsourceMinDimension.ValueAsUInt : -1,
                                                               //opt.MapsourceOverviewNo.IsSet ? (int)opt.MapsourceOverviewNo.ValueAsUInt : -1,

                                                               new SortedSet<int>(new int[] {
                                                                     0x0400,     // Großstadt
                                                                     0x0600,     // Stadt
                                                                     //0x0900      // Vorort
                                                               }),
                                                               new SortedSet<int>(new int[] {
                                                                     0x00100,     // Autobahn
                                                                     0x00200,     // autobahnähnliche Straße
                                                                     0x00300,     // Bundesstraße
                                                                     0x00400,     // Land-, (Staats-,) oder sehr gut ausgebaute Kreisstraße
                                                                     0x01F00,     // Fluß
                                                               }),
                                                               new SortedSet<int>(new int[] {
                                                                     //0x00300,     // Stadt
                                                                     0x03200,     // Wasser
                                                                     0x05000,     // Wald
                                                               }),

                                                               opt.MapsourceTDBfile.IsSet ? opt.MapsourceTDBfile.ValueAsString : null,
                                                               opt.MapsourceOverviewfile.IsSet ? opt.MapsourceOverviewfile.ValueAsString : null,
                                                               opt.MapsourceTYPfile.IsSet ? opt.MapsourceTYPfile.ValueAsString : null,
                                                               opt.MapsourceMDXfile.IsSet ? opt.MapsourceMDXfile.ValueAsString : null,
                                                               opt.MapsourceMDRfile.IsSet ? opt.MapsourceMDRfile.ValueAsString : null,

                                                               opt.MapsourceNoTDBfile.IsSet ? opt.MapsourceNoTDBfile.ValueAsBool : false,
                                                               opt.MapsourceNoOverviewfile.IsSet ? opt.MapsourceNoOverviewfile.ValueAsBool : false,
                                                               opt.MapsourceNoTYPfile.IsSet ? opt.MapsourceNoTYPfile.ValueAsBool : false,
                                                               opt.MapsourceNoMDXfile.IsSet ? opt.MapsourceNoMDXfile.ValueAsBool : false,
                                                               opt.MapsourceNoMDRfile.IsSet ? opt.MapsourceNoMDRfile.ValueAsBool : false,
                                                               opt.MapsourceNoInstfiles.IsSet ? opt.MapsourceNoInstfiles.ValueAsBool : false,

                                                               opt.Version.IsSet ? (ushort)opt.Version.ValueAsInt : (ushort)100,
                                                               opt.Routable.IsSet ? (byte)opt.Routable.ValueAsInt : (byte)0,
                                                               opt.MapFamilyName.IsSet ? opt.MapFamilyName.ValueAsString : null,
                                                               opt.MapSeriesName.IsSet ? opt.MapSeriesName.ValueAsString : null,
                                                               opt.Description.IsSet ? opt.Description.ValueAsString : null,
                                                               CopyrightCodes, CopyrightWhereCodes, CopyrightText,
                                                               opt.HighestRoutable.IsSet ? (byte)(opt.HighestRoutable.ValueAsInt & 0xFF) : (byte)0,
                                                               opt.HasDEM.IsSet ? (byte)(opt.HasDEM.ValueAsInt & 0xFF) : (byte)0,
                                                               opt.HasProfile.IsSet ? (byte)(opt.HasProfile.ValueAsInt & 0xFF) : (byte)0,
                                                               opt.MaxCoordBits4Overview.IsSet ? (byte)(opt.MaxCoordBits4Overview.ValueAsInt & 0xFF) : (byte)0,

                                                               Output,
                                                               opt.OutputOverwrite);

                  }
                  break;

               case Options.ToDoType.AnalyzingTypes:
               case Options.ToDoType.AnalyzingTypesLong:
                  AnalyzeTypes analyze = new AnalyzeTypes();
                  foreach (string file in InputFiles)
                     analyze.AnalyzingTypes(file);
                  analyze.ShowAnalyzingTypesResult(opt.ToDo == Options.ToDoType.AnalyzingTypesLong);
                  break;

            }
         } catch (Exception ex) {
            Console.Error.WriteLine("Fehler: " + ex.Message);
         }
      }

      /// <summary>
      /// erzeugt die Liste <see cref="InputFiles"/> aller Input-Dateien
      /// <para>Wildcards im Dateinamen (NICHT Pfad) werden ausgewertet.</para>
      /// <para>Mehrfach angegebene Dateien werden entfernt.</para>
      /// <para>Es werden die vollständigen Dateipfade ergänzt.</para>
      /// </summary>
      /// <returns>true, wenn min. 1 Datei angegeben ist</returns>
      bool PrepareInputData(bool withsubdirs) {
         InputFiles = new List<string>();

         // Wenn ein Pfad oder eine Maske angeben ist, werden die einzelnen Dateien ermittelt.
         foreach (string inp in opt.Input) {
            string file = Path.GetFileName(inp);

            if (file.IndexOfAny(wildcards) >= 0) {    // Wildcards beziehen sich nur auf Dateien
               string filepath = Path.GetDirectoryName(inp);
               if (filepath == "")
                  filepath = ".";
               InputFiles.AddRange(Directory.GetFiles(filepath, file, withsubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));
               continue;
            }

            file = Path.GetFullPath(inp);          // <- löst auch eine Exception bei ungültigem Pfadaufbau aus (illegale Zeichen o.ä.)

            if (Directory.Exists(file)) {
               InputFiles.AddRange(Directory.GetFiles(file, "*.*", withsubdirs ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly));  // alle Dateien im Verzeichnis und in den Unterverzeichnissen
               continue;
            }

            if (File.Exists(file)) {
               InputFiles.Add(file);
               continue;
            }

            Console.Error.WriteLine("Der Input '" + inp + "' existiert nicht und wird ignoriert.");
         }

         // Dubletten entfernen
         for (int i = 0; i < InputFiles.Count; i++) {
            string txt = InputFiles[i].ToUpper();
            for (int j = i + 1; j < InputFiles.Count; j++)
               if (InputFiles[j].ToUpper() == txt)
                  InputFiles.RemoveAt(j--);
         }

         if (InputFiles.Count == 0)
            Console.Error.WriteLine("Keine Daten zur Verarbeitung angegeben.");

         Console.WriteLine("Anzahl der Eingabedateien: {0}", InputFiles.Count);

         return InputFiles.Count > 0;
      }

      /// <summary>
      /// setzt <see cref="Output"/> mit dem Ausgabeziel mit vollständiger Pfadangabe
      /// <para>Ist das Ausgabeziel eine Datei, wird sie bei erlaubtem Schreiben zunächst gelöscht.</para>
      /// </summary>
      /// <returns></returns>
      bool PrepareOutputData() {
         if (opt.Output.Length == 0) {
            Console.Error.WriteLine("Kein Ziel der Verarbeitung angegeben.");
            return false;
         }

         Output = Path.GetFullPath(opt.Output);       // <- löst auch eine Exception bei ungültigem Pfadaufbau aus (illegale Zeichen o.ä.)

         if (opt.OutputOverwrite) {                   // wenn das Ziel schon existiert, wird es gelöscht
            if (!Directory.Exists(Output)) {
               if (File.Exists(Output))
                  if (opt.OutputOverwrite)
                     File.Delete(Output);
                  else {
                     Console.Error.WriteLine("Die Ziel-Datei darf nicht gelöscht werden.");
                     return false;
                  }
            }
            //if (Directory.Exists(Output))
            //   Directory.Delete(Output, true);       // löscht rekursiv
         }

         return true;
      }

      /// <summary>
      ///  zerlegt die Garmin-Eingabedatei (IMG oder GMP) in einzelne Dateien im angegebenen Pfad
      /// </summary>
      /// <param name="imgfile">IMG- oder GMP-Datei</param>
      /// <param name="outputpath">Zielpfad</param>
      /// <param name="overwrite">wenn true, werden ev. vorhanden Dateien überschrieben</param>
      /// <param name="recursive">wenn true, erfolgt die Zerlegung rekursiv</param>
      /// <param name="jointiles">wenn true, werden zusammengehörende TRE-, LBL-, RGN-, NET-, NOD-, DEM- und MAR-Dateien wieder zu IMG-Dateien verbunden</param>
      void Split(string imgfile, string outputpath, bool overwrite, bool recursive, bool jointiles) {
         try {

            string ext = Path.GetExtension(imgfile).ToUpper();

            if (ext == ".IMG") {

               SimpleFilesystem sf = new SimpleFilesystem();
               using (BinaryReaderWriter br = new BinaryReaderWriter(File.OpenRead(imgfile))) {
                  sf.Read(br);
                  Console.WriteLine(string.Format("{0} Dateie{1} in '{2}'", sf.FileCount, sf.FileCount == 1 ? "" : "n", imgfile));

                  if (!Directory.Exists(outputpath))
                     Directory.CreateDirectory(outputpath);

                  // sortierte Liste (wegen aufeinanderfolgender Basisnamen) alle vorhandenen Dateinamen erzeugen
                  SortedSet<string> files = new SortedSet<string>();
                  for (int i = 0; i < sf.FileCount; i++)
                     files.Add(sf.Filename(i));

                  string lastbasefilename = "";
                  foreach (string file in files) {
                     string newbasefilename = Path.GetFileNameWithoutExtension(file);

                     bool tileext = HasExtension4TileImg(file, false);
                     if (tileext && jointiles) {
                        if (lastbasefilename != newbasefilename) { // nur wenn neuer Basename; die anderen Dateien mit dem gleichen Basename sind schon verarbeitet
                           Console.WriteLine("Datei '" + Path.Combine(outputpath, newbasefilename) + ".img' wird erzeugt ...");
                           CreateNewImgfile(sf, newbasefilename, outputpath, overwrite);
                        }
                     } else {
                        string destfilename = Path.Combine(outputpath, file);

                        if (tileext) // dann im Unterverzeichnis erzeugen
                           destfilename = Path.Combine(outputpath, GetNumber4Basefilename(file.Substring(0, 8)).ToString("d8"), file);
                        Console.WriteLine("Datei '" + destfilename + "' wird erzeugt ...");
                        FileCopy(sf, file, destfilename, overwrite);

                        string ext2 = Path.GetExtension(destfilename).ToUpper();
                        if (recursive && (ext2 == ".IMG" || ext2 == ".GMP")) {
                           //Split(file, destfilename, overwrite, recursive, jointiles);
                           Split(destfilename, destfilename.Substring(0, destfilename.Length - 4), overwrite, recursive, jointiles);
                        }
                     }

                     lastbasefilename = newbasefilename;
                  }
                  br.Dispose();
               }

            } else if (ext == ".GMP") {

               int no = GetNumber4Basefilename(Path.GetFileNameWithoutExtension(imgfile));
               string basename = Path.Combine(no > 0 ?
                                                no.ToString("d8") :
                                                Path.GetFileNameWithoutExtension(imgfile));

               if (!Directory.Exists(outputpath))
                  Directory.CreateDirectory(outputpath);

               using (BinaryReaderWriter br = new BinaryReaderWriter(File.OpenRead(imgfile))) {
                  StdFile_GMP gmp = new StdFile_GMP();
                  gmp.Read(br, true);

                  if (jointiles) {
                     string destfile = Path.Combine(outputpath, Path.GetFileNameWithoutExtension(imgfile)) + ".img";
                     Console.WriteLine("Datei '" + destfile + "' wird erzeugt ...");
                     using (BinaryReaderWriter bwimg = new BinaryReaderWriter(File.OpenWrite(destfile))) {
                        using (SimpleFilesystem sfimg = new SimpleFilesystem()) {
                           string ext2 = "";
                           for (int i = 0; i < 6; i++) {
                              StdFile std = null;
                              switch (i) {
                                 case 0:
                                    ext2 = ".TRE";
                                    std = gmp.TRE;
                                    break;

                                 case 1:
                                    ext2 = ".LBL";
                                    std = gmp.LBL;
                                    break;

                                 case 2:
                                    ext2 = ".RGN";
                                    std = gmp.RGN;
                                    break;

                                 case 3:
                                    ext2 = ".NET";
                                    std = gmp.NET;
                                    break;

                                 case 4:
                                    ext2 = ".NOD";
                                    std = gmp.NOD;
                                    break;

                                 case 5:
                                    ext2 = ".DEM";
                                    std = gmp.DEM;
                                    break;

                                 case 6:
                                    ext2 = ".MAR";
                                    std = gmp.MAR;
                                    break;

                              }
                              if (std != null) {
                                 BinaryReaderWriter bwfile = null, bwfs = null;
                                 try {
                                    bwfile = new BinaryReaderWriter();
                                    std.Write(bwfile, 0, 0, 0, 0, false);
                                    string filename = basename + ext2;
                                    sfimg.FileAdd(filename, (uint)bwfile.Length);
                                    Console.WriteLine(string.Format("füge '{0}' hinzu ({1} Bytes)...", filename, bwfile.Length));
                                    bwfs = sfimg.GetBinaryReaderWriter4File(filename);
                                    bwfile.CopyTo(bwfs);
                                 } finally {
                                    bwfile?.Dispose();
                                    bwfs?.Dispose();
                                 }
                              }
                           }
                           sfimg.Write(bwimg);
                        }
                     }

                  } else {

                     StdFile[] stdfile = new StdFile[] { gmp.TRE, gmp.RGN, gmp.LBL, gmp.NET, gmp.NOD, gmp.DEM, gmp.MAR };
                     string[] extension = new string[] { "tre", "rgn", "lbl", "net", "nod", "dem", "mar" };
                     for (int i = 0; i < stdfile.Length; i++) {
                        if (stdfile[i] != null) {
                           string destfile = Path.Combine(outputpath, basename + "." + extension[i]);
                           if (File.Exists(destfile) && !overwrite)
                              Console.Error.WriteLine("Die Ziel-Datei '" + destfile + "' darf nicht gelöscht werden.");
                           else {
                              File.Delete(destfile);
                              using (BinaryReaderWriter bw = new BinaryReaderWriter(File.Create(destfile))) {
                                 Console.WriteLine("Datei '" + destfile + "' wird erzeugt ...");
                                 stdfile[i].Write(bw, 0, stdfile[i].Headerlength, 0, 0, !stdfile[i].RawRead);
                              }
                           }
                        }
                     }

                  }

                  br.Dispose();
               }

            } else
               throw new Exception(string.Format("Es können nur IMG- und GMP-Dateien zerlegt werden."));

         } catch (Exception ex) {
            Console.Error.WriteLine("Fehler beim Zerlegen der Datei '" + imgfile + "': " + ex.Message);
         }
      }

      /// <summary>
      /// verbindet die Dateien der Liste zu einer IMG- oder GMP-Datei
      /// </summary>
      /// <param name="files">zu verbindende Dateien</param>
      /// <param name="outputpath">Zieldatei</param>
      /// <param name="description"></param>
      /// <param name="overwrite"></param>
      /// <param name="fordevice">expliziet für Device</param>
      /// <param name="fortile">expliziet als Kartenkachel</param>
      void Join(IList<string> files, string outputpath, string description, bool overwrite, bool fordevice, bool fortile) {
         try {

            string ext = Path.GetExtension(outputpath).ToUpper();
            if (!(ext == ".IMG" || ext == ".GMP"))
               throw new Exception("Die Zieldatei muss eine IMG- oder GMP-Datei sein.");

            List<string> filelist;
            if (PrepareFilelist4Join(files, out filelist, fordevice, fortile)) {

               Console.WriteLine(string.Format("erzeuge Geräte-IMG-Datei '{0}' ...", outputpath));
               if (ext != ".IMG")
                  throw new Exception("Die Zieldatei muss eine IMG-Datei sein.");
               CreateNewImgfile(filelist, outputpath, description, overwrite);

            } else {

               if (ext == ".IMG") {
                  Console.WriteLine(string.Format("erzeuge Kachel-IMG-Datei '{0}' ...", outputpath));
                  CreateNewImgfile(filelist, outputpath, description, overwrite);
               } else {
                  Console.WriteLine(string.Format("erzeuge GMP-Datei '{0}' ...", outputpath));

                  StdFile_GMP gmp = new StdFile_GMP();
                  gmp.Copyright.Add("erzeugt mit GmTool");

                  for (int i = 0; i < filelist.Count; i++) {
                     StdFile std = null;
                     ext = Path.GetExtension(filelist[i]).ToUpper();
                     if (ext == ".TRE")
                        std = gmp.TRE = new StdFile_TRE();
                     else if (ext == ".LBL")
                        std = gmp.LBL = new StdFile_LBL();
                     else if (ext == ".RGN")
                        std = gmp.RGN = new StdFile_RGN(gmp.TRE);
                     else if (ext == ".NET") {
                        std = gmp.NET = new StdFile_NET();
                        gmp.NET.Lbl = gmp.LBL;
                     } else if (ext == ".NOD")
                        std = gmp.NOD = new StdFile_NOD();
                     else if (ext == ".DEM")
                        std = gmp.DEM = new StdFile_DEM();
                     else if (ext == ".MAR")
                        std = gmp.MAR = new StdFile_MAR();
                     else
                        throw new Exception("Interner Fehler.");
                     if (std != null) {
                        Console.WriteLine(string.Format("füge '{0}' hinzu ...", filelist[i]));
                        std.Read(new BinaryReaderWriter(filelist[i], true), true);
                     }
                  }
                  Console.WriteLine(string.Format("erzeuge '{0}' ...", outputpath));
                  gmp.Write(new BinaryReaderWriter(outputpath, false, true, true));

               }
            }

         } catch (Exception ex) {
            Console.Error.WriteLine("Fehler beim Erzeugen der Datei '" + outputpath + "': " + ex.Message);
         }
      }

      /// <summary>
      /// erzeugt eine überprüfte Dateiliste für die Erzeugung einer IMG- bzw. GMP-Datei
      /// </summary>
      /// <param name="files"></param>
      /// <param name="filelist"></param>
      /// <param name="fordevice">expliziet für Device</param>
      /// <param name="fortile">expliziet als Kartenkachel</param>
      /// <returns>true, wenn für ein Device-IMG, sonst für Tile-IMG/GMP</returns>
      bool PrepareFilelist4Join(IList<string> files, out List<string> filelist, bool fordevice, bool fortile) {
         filelist = null;

         // Art und Häufigkeit des Auftretens der Erweiterung ermitteln
         SortedDictionary<string, int> extsample = new SortedDictionary<string, int>();
         foreach (string file in files) {
            string ext = Path.GetExtension(file).ToUpper();
            if (!extsample.ContainsKey(ext))
               extsample.Add(ext, 1);
            else
               extsample[ext] += 1;
         }

         // IMG-Typ bestimmen
         if (!fordevice && !fortile) {
            if (extsample.ContainsKey(".IMG")) {  // zwangsläufig Device-IMG
               fordevice = true;
               fortile = false;
            } else {
               fordevice = false;
               fortile = true;
            }
         } else if (fordevice)
            fortile = false;
         else
            fortile = true;

         if (fordevice) { // zwangsläufig Device-IMG

            // Eine Geräte-IMG darf beliebige Dateien enthalten.

            //foreach (string ext in TILEIMGEXTENSIONS) {
            //   if (extsample.ContainsKey(ext))
            //      throw new Exception(string.Format("Ein Geräte-IMG darf keine Datei des Typs '{0}' enthalten.", ext));
            //}
         } else {
            SortedSet<string> sortedtileext = new SortedSet<string>(TILEIMGEXTENSIONS);
            foreach (var item in extsample) {
               if (!sortedtileext.Contains(item.Key))
                  throw new Exception(string.Format("Ein Kachel-IMG darf keine Datei des Typs '{0}' enthalten.", item.Key));
            }
            foreach (string ext in TILEIMGEXTENSIONS) {
               if (extsample.ContainsKey(ext) &&
                   extsample[ext] > 1)
                  throw new Exception(string.Format("Ein Kachel-IMG darf nur EINE Datei des Typs '{0}' enthalten.", ext));
            }
            if (extsample.ContainsKey(".GMP") &&
                files.Count > 1)
               if (!extsample.ContainsKey(".SRT"))
                  throw new Exception("Ein Kachel-IMG mit einer GMP-Datei darf außer einer SRT-Dateie keine weiteren Dateien enthalten.");
         }

         // Test auf eindeutige Dateinamen (ev. Dateien aus verschiedenen Verzeichnissen) und Namenslänge
         SortedSet<string> filenamesample = new SortedSet<string>();
         foreach (string file in files) {
            string filename = Path.GetFileName(file).ToUpper();
            if (Path.GetFileNameWithoutExtension(filename).Length > 8)
               throw new Exception(string.Format("Der Basisname der Datei '{0}' ist zu lang (max. 8 Zeichen).", file));
            if (Path.GetExtension(filename).Length > 4)
               throw new Exception(string.Format("Die Erweiterung der Datei '{0}' ist zu lang (max. 3 Zeichen).", file));
            if (!filenamesample.Contains(filename))
               filenamesample.Add(filename);
            else
               throw new Exception(string.Format("Die Dateien '{0}' führt in der IMG-Datei zu einem Namenskonflikt ('{1}').", file, filename));
         }

         // sortierte Namensliste erzeugen
         filelist = new List<string>();
         if (fordevice) {

            SortedSet<string> imgfiles = new SortedSet<string>();
            SortedSet<string> otherfiles = new SortedSet<string>();
            foreach (string file in files) {
               if (Path.GetExtension(file).ToUpper() == ".IMG")
                  imgfiles.Add(file);
               else
                  otherfiles.Add(file);
            }
            foreach (string file in imgfiles)
               filelist.Add(file);
            foreach (string file in otherfiles)
               filelist.Add(file);

         } else {

            string basename = null;
            // es ist bereits getestet, dass max. 1 Datei je Typ ex.; Test auf identischen Basisnamen
            foreach (string ext in TILEIMGEXTENSIONS) {
               foreach (string file in files) {
                  string newbasename = Path.GetFileNameWithoutExtension(file);
                  if (!string.IsNullOrEmpty(basename) &&
                      basename != newbasename)
                     throw new Exception(string.Format("In einer Kachel-IMG-Datei müssen alle Dateien den gleichen Basisnamen haben ({0} <==> {1}).", basename, file));
                  if (Path.GetExtension(file).ToUpper() == ext)
                     filelist.Add(file);
               }
            }

         }

         return fordevice;
      }

      #region Änderung von Eigenschaften

      /// <summary>
      /// ändert die Eigenschaften einer Datei
      /// </summary>
      /// <param name="srcfile">Datei</param>
      /// <param name="outputpath">Ausgabeziel</param>
      /// <param name="overwrite">wenn true wird eine ev. vorhandene Dateie überschrieben</param>
      /// <param name="opt">Optionen</param>
      void ChangeProperties(string srcfile, string outputpath, bool overwrite, Options opt) {
         string destfile = string.IsNullOrEmpty(outputpath) ?
                              srcfile :
                              Directory.Exists(outputpath) ?
                                 Path.Combine(outputpath, Path.GetFileName(srcfile)) :
                                 outputpath;
         if (srcfile != destfile && File.Exists(destfile) && !overwrite)
            Console.Error.WriteLine("Die Ziel-Datei '" + destfile + "' darf nicht überschrieben werden.");
         else {
            string ext = Path.GetExtension(srcfile).ToUpper();

            if (ext == ".IMG")
               ChangeIMG(srcfile, destfile, opt);
            else if (ext == ".TDB")
               ChangeTDB(srcfile, destfile, opt);
            else if (ext == ".MDX")
               ChangeMDX(srcfile, destfile, opt);
            else if (ext == ".MPS")
               ChangeMPS(srcfile, destfile, opt);
            //else if (ext == ".DEM")
            //   ;
            //else if (ext == ".GMP")
            //   ;
            //else if (ext == ".LBL")
            //   ; // nichts
            //else if (ext == ".MDR")
            //   ; // nichts
            //else if (ext == ".NET")
            //   ; // nichts
            else if (ext == ".RGN")
               ChangeRGN(srcfile, destfile, opt);
            //else if (ext == ".SRT")
            //   ; // nichts
            else if (ext == ".TRE")
               ChangeTRE(srcfile, destfile, opt);
            else if (ext == ".TYP")
               ChangeTYP(srcfile, destfile, opt);
            else {
               Console.Error.WriteLine("Keine Änderungen möglich für Dateityp: '" + destfile + "'");
            }
         }
      }

      void ChangeTYP(string srcfile, string destfile, Options opt) {
         if (opt.PID.IsSet ||
             opt.FID.IsSet ||
             opt.Codepage.IsSet) {
            StdFile_TYP file = new StdFile_TYP();
            Console.WriteLine("lese Datei '" + srcfile + "'");
            BinaryReaderWriter br = new BinaryReaderWriter(srcfile, true);
            file.Read(br);
            br.Dispose();

            if (opt.PID.IsSet)
               file.ProductID = (ushort)opt.PID.ValueAsInt;

            if (opt.FID.IsSet)
               file.FamilyID = (ushort)opt.FID.ValueAsInt;

            if (opt.Codepage.IsSet)
               file.Codepage = (ushort)opt.Codepage.ValueAsInt;

            Console.WriteLine("erzeuge Datei '" + destfile + "'");
            file.Write(new BinaryReaderWriter(destfile, true, true, true), 0, 0, 0, 0, false);
         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (PID, FID, Codepage) angegeben.");
         }
      }

      void ChangeMDX(string srcfile, string destfile, Options opt) {
         if (opt.PID.IsSet ||
             opt.FID.IsSet) {
            File_MDX file = new File_MDX();
            Console.WriteLine("lese Datei '" + srcfile + "'");
            BinaryReaderWriter br = new BinaryReaderWriter(srcfile, true);
            file.Read(br);
            br.Dispose();

            if (opt.PID.IsSet)
               for (int i = 0; i < file.Maps.Count; i++)
                  file.Maps[i].ProductID = (ushort)opt.PID.ValueAsInt;

            if (opt.FID.IsSet)
               for (int i = 0; i < file.Maps.Count; i++)
                  file.Maps[i].FamilyID = (ushort)opt.FID.ValueAsInt;

            Console.WriteLine("erzeuge Datei '" + destfile + "'");
            file.Write(new BinaryReaderWriter(destfile, true, true, true));
         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (PID, FID) angegeben.");
         }
      }

      void ChangeMPS(string srcfile, string destfile, Options opt) {
         if (opt.PID.IsSet ||
             opt.FID.IsSet) {
            File_MPS file = new File_MPS();
            Console.WriteLine("lese Datei '" + srcfile + "'");
            BinaryReaderWriter br = new BinaryReaderWriter(srcfile, true);
            file.Read(br);
            br.Dispose();

            if (opt.PID.IsSet)
               for (int i = 0; i < file.Maps.Count; i++)
                  file.Maps[i].ProductID = (ushort)opt.PID.ValueAsInt;

            if (opt.FID.IsSet)
               for (int i = 0; i < file.Maps.Count; i++)
                  file.Maps[i].FamilyID = (ushort)opt.FID.ValueAsInt;

            Console.WriteLine("erzeuge Datei '" + destfile + "'");
            file.Write(new BinaryReaderWriter(destfile, true, true, true));
         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (PID, FID) angegeben.");
         }
      }

      void ChangeIMG(string srcfile, string destfile, Options opt) {
         if (opt.Description.IsSet) {
            if (srcfile != destfile) { // nicht "in place": da sich die Dateigröße nicht ändert und nur der Dateiheader verändert wird, ist eine Dateikopie am einfachsten
               Console.WriteLine("erzeuge Datei '" + destfile + "'");
               File.Copy(srcfile, destfile);
               srcfile = destfile;
            }

            Console.WriteLine("lese Datei '" + srcfile + "'");
            BinaryReaderWriter brw = new BinaryReaderWriter(srcfile, true, true);

            SimpleFilesystem fs = new SimpleFilesystem();
            fs.Read(brw);

            if (opt.Description.IsSet)
               fs.ImgHeader.Description = opt.Description.ValueAsString;
            // auch fs.ImgHeader.MapsourceFlag wäre denkbar

            Console.WriteLine("ändere Datei '" + destfile + "'");
            brw.Seek(0);
            fs.ImgHeader.Write(brw);

            brw.Dispose();
         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (Description) angegeben.");
         }
      }

      void ChangeTDB(string srcfile, string destfile, Options opt) {
         if (opt.PID.IsSet ||
             opt.FID.IsSet ||
             opt.Codepage.IsSet ||
             opt.Version.IsSet ||
             opt.Routable.IsSet ||
             opt.MapFamilyName.IsSet ||
             opt.MapSeriesName.IsSet ||
             opt.Description.IsSet ||
             opt.TDBCopyrightText.Count > 0 ||
             opt.HighestRoutable.IsSet ||
             opt.HasDEM.IsSet ||
             opt.HasProfile.IsSet ||
             opt.MaxCoordBits4Overview.IsSet) {

            File_TDB file = new File_TDB();
            Console.WriteLine("lese Datei '" + srcfile + "'");
            BinaryReaderWriter br = new BinaryReaderWriter(srcfile, true);
            file.Read(br);
            br.Dispose();

            if (opt.PID.IsSet)
               file.Head.ProductID = (ushort)opt.PID.ValueAsInt;
            if (opt.FID.IsSet)
               file.Head.FamilyID = (ushort)opt.FID.ValueAsInt;
            if (opt.Codepage.IsSet)
               file.Head.CodePage = (uint)opt.Codepage.ValueAsInt;
            if (opt.Version.IsSet)
               file.Head.ProductVersion = (ushort)opt.Version.ValueAsInt;
            if (opt.Routable.IsSet)
               file.Head.Routable = (byte)opt.Routable.ValueAsInt;
            if (opt.MapFamilyName.IsSet)
               file.Head.MapFamilyName = opt.MapFamilyName.ValueAsString;
            if (opt.MapSeriesName.IsSet)
               file.Head.MapSeriesName = opt.MapSeriesName.ValueAsString;
            if (opt.Description.IsSet)
               file.Mapdescription.Text = opt.Description.ValueAsString;
            if (opt.HighestRoutable.IsSet)
               file.Head.HighestRoutable = (byte)(opt.HighestRoutable.ValueAsInt & 0xFF);
            if (opt.HasDEM.IsSet)
               file.Head.HasDEM = (byte)(opt.HasDEM.ValueAsInt & 0xFF);
            if (opt.HasProfile.IsSet)
               file.Head.HasProfileInformation = (byte)(opt.HasProfile.ValueAsInt & 0xFF);
            if (opt.MaxCoordBits4Overview.IsSet)
               file.Head.MaxCoordbits4Overview = (byte)(opt.MaxCoordBits4Overview.ValueAsInt & 0xFF);

            if (opt.TDBCopyrightText.Count > 0) {
               List<File_TDB.SegmentedCopyright.Segment> newsegments = new List<File_TDB.SegmentedCopyright.Segment>();

               for (int i = 0; i < Math.Max(file.Copyright.Segments.Count, opt.TDBCopyrightText.Count); i++) {
                  File_TDB.SegmentedCopyright.Segment segment = i < file.Copyright.Segments.Count ?
                                                                           new File_TDB.SegmentedCopyright.Segment(file.Copyright.Segments[i]) :
                                                                           new File_TDB.SegmentedCopyright.Segment(File_TDB.SegmentedCopyright.Segment.CopyrightCodes.CopyrightInformation,
                                                                                                                   File_TDB.SegmentedCopyright.Segment.WhereCodes.ProductInformationAndPrinting,
                                                                                                                   "");
                  if (i >= opt.TDBCopyrightText.Count) // altes Segment unverändert übernehmen
                     newsegments.Add(segment);
                  else {
                     if (opt.TDBCopyrightText[i].Value == null) {
                        if (!opt.TDBCopyrightText[i].IsSet) // Segment löschen
                           continue;
                     } else
                        segment.Copyright = opt.TDBCopyrightText[i].ValueAsString;

                     File_TDB.SegmentedCopyright.Segment.CopyrightCodes ccode = (File_TDB.SegmentedCopyright.Segment.CopyrightCodes)opt.TDBCopyrightCodes[i].ValueAsInt;
                     if (ccode != File_TDB.SegmentedCopyright.Segment.CopyrightCodes.Unknown)
                        segment.CopyrightCode = ccode;

                     File_TDB.SegmentedCopyright.Segment.WhereCodes wcode = (File_TDB.SegmentedCopyright.Segment.WhereCodes)opt.TDBCopyrightWhereCodes[i].ValueAsInt;
                     if (wcode != File_TDB.SegmentedCopyright.Segment.WhereCodes.Unknown)
                        segment.WhereCode = wcode;

                     newsegments.Add(segment);
                  }
               }
               file.Copyright.Segments = newsegments;
            }

            Console.WriteLine("erzeuge Datei '" + destfile + "'");
            using (BinaryReaderWriter bw = new BinaryReaderWriter(destfile, false, true, true)) {
               file.Write(bw);
               bw.Dispose();
            }

         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (PID, FID, Codepage, Version, Routable, Mapname, Mapsetname, Description, Copyright) angegeben.");
         }
      }

      void ChangeTRE(string srcfile, string destfile, Options opt) {
         if (opt.Priority.IsSet) {
            StdFile_TRE file = new StdFile_TRE();
            Console.WriteLine("lese Datei '" + srcfile + "'");
            BinaryReaderWriter br = new BinaryReaderWriter(srcfile, true);
            file.Read(br);
            br.Dispose();

            file.DisplayPriority = opt.Priority.ValueAsInt;

            Console.WriteLine("erzeuge Datei '" + destfile + "'");
            file.Write(new BinaryReaderWriter(destfile, true, true, true), 0, 0, 0, 0, false);
         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (priority) angegeben.");
         }
      }

      void ChangeRGN(string srcfile, string destfile, Options opt) {
         if (opt.Transparent.IsSet) {
            StdFile_TRE tre = new StdFile_TRE();
            string trefilename = Path.Combine(Path.GetDirectoryName(srcfile), Path.GetFileNameWithoutExtension(srcfile)) + ".tre";
            if (File.Exists(trefilename)) {
               Console.WriteLine("lese Datei '" + trefilename + "'");
               BinaryReaderWriter brtre = new BinaryReaderWriter(trefilename, true);
               if (brtre != null)
                  tre.Read(brtre, false);
               brtre.Dispose();

               StdFile_RGN file = new StdFile_RGN(tre);
               Console.WriteLine("lese Datei '" + srcfile + "'");
               BinaryReaderWriter br = new BinaryReaderWriter(srcfile, true);
               file.Read(br);
               br.Dispose();

               int polycount = 0;
               if (opt.Transparent.ValueAsInt != 0) {

                  for (int i = 0; i < file.SubdivList.Count; i++) { // in jeder Subdiv ..
                     StdFile_RGN.SubdivData sd = file.SubdivList[i];
                     for (int j = sd.AreaList.Count - 1; j >= 0; j--) // .. jedes Polygon ..
                        if (sd.AreaList[j].Typ == 0x4b) { // .. mit dem Typ 0x4B (Hintergrund) ..
                           sd.AreaList.RemoveAt(j); // .. entfernen
                           polycount++;
                        }
                  }
                  Console.WriteLine("Es wurden " + polycount.ToString() + " Polygone vom Typ 0x4B entfernt.");

               } else {

                  for (int i = 0; i < file.SubdivList.Count; i++) { // in jeder Subdiv ..
                     StdFile_RGN.SubdivData sd = file.SubdivList[i];
                     StdFile_TRE.SubdivInfoBasic sdi = tre.SubdivInfoList[i];

                     bool bBackPolyExist = false;
                     for (int j = 0; j < sd.AreaList.Count; j++) // .. Test auf Polygon 0x4B 
                        if (sd.AreaList[j].Typ == 0x4b) {
                           bBackPolyExist = true;
                           break;
                        }

                     if (!bBackPolyExist) {
                        // das umgebende Rechteck aller Koordinaten der Subdiv in internen Koordinaten und bzgl. des Subdiv-Mittelpunktes ermitteln

                        int coordbits = file.TREFile.SymbolicScaleDenominatorAndBitsLevel.Bits4SubdivIdx1(i + 1);
                        Bound b = sd.GetBound4Deltas(coordbits, sdi.Center);
                        if (b != null) {
                           List<MapUnitPoint> p = new List<MapUnitPoint>();
                           p.Add(new MapUnitPoint(b.Left, b.Top));
                           p.Add(new MapUnitPoint(b.Right, b.Top));
                           p.Add(new MapUnitPoint(b.Right, b.Bottom));
                           p.Add(new MapUnitPoint(b.Left, b.Bottom));

                           StdFile_RGN.RawPolyData pd = new StdFile_RGN.RawPolyData(true);
                           pd.Typ = 0x4b;
                           pd.SetMapUnitPoints(coordbits, sdi.Center, p);

                           sd.AreaList.Add(pd);
                           polycount++;
                        }
                     }
                  }
                  Console.WriteLine("Es wurden " + polycount.ToString() + " Polygone vom Typ 0x4B hinzugefügt.");

               }

               Console.WriteLine("erzeuge Datei '" + destfile + "'");
               file.Write(new BinaryReaderWriter(destfile, true, true, true), 0, 0, 0, 0, true);
               trefilename = Path.Combine(Path.GetDirectoryName(destfile), Path.GetFileNameWithoutExtension(destfile)) + ".tre";
               Console.WriteLine("erzeuge Datei '" + trefilename + "'");
               tre.Write(new BinaryReaderWriter(trefilename, true, true, true), 0, 0, 0, 0, true);

            } else
               Console.Error.WriteLine("Zur Datei '" + srcfile + "' existiert keine passende TRE-Datei '" + trefilename + "'.");
         } else {
            Console.Error.WriteLine("Keine veränderbare Datei-Eigenschaft für '" + srcfile + "' (transparent) angegeben.");
         }
      }

      #endregion

      #region Hilfsfunktionen

      /// <summary>
      /// sortierte Dateiextensionen der Dateien die in einer Tile-IMG-Datei zusammengefasst werden können
      /// </summary>
      readonly string[] TILEIMGEXTENSIONS = new string[] { ".TRE", ".LBL", ".RGN", ".NET", ".NOD", ".DEM", ".MAR", ".GMP", ".SRT" };

      /// <summary>
      /// liefert true, wenn die Datei eine Extension für eine Kachel-IMG-Einzeldatei hat
      /// </summary>
      /// <param name="file"></param>
      /// <param name="withgmp">wenn true, liefert eine GMP-Datei auch true</param>
      /// <returns></returns>
      bool HasExtension4TileImg(string file, bool withgmp = true) {
         string ext = Path.GetExtension(file).ToUpper();
         for (int i = 0; i < TILEIMGEXTENSIONS.Length; i++)
            if (TILEIMGEXTENSIONS[i] == ext)
               if (!withgmp && ext == ".GMP")
                  return false;
               else
                  return true;
         return false;
      }

      /// <summary>
      /// kopiert die Dateien mit dem Basisnamen aus dem <see cref="SimpleFilesystem"/> in eine neu erzeugte IMG-Datei
      /// deren Name auf dem Basisnamen basiert
      /// <para>Das sind alle Daten für eine Single-IMG-Datei.</para>
      /// </summary>
      /// <param name="src_sf"><see cref="SimpleFilesystem"/> der Quell-IMG-Datei</param>
      /// <param name="basename">Basisname der Dateien</param>
      /// <param name="tileimgpath">Pfad der neuen Kachel-IMG-Datei</param>
      /// <param name="overwrite">true, wenn eine ev. schon ex. IMG-Datei überschrieben werden soll</param>
      void CreateNewImgfile(SimpleFilesystem src_sf, string basename, string tileimgpath, bool overwrite) {
         List<string> files = new List<string>();
         foreach (string ext in TILEIMGEXTENSIONS) {
            string file = basename + ext;
            if (src_sf.FilenameExist(file))
               files.Add(file);
         }

         if (files.Count > 0)
            CreateNewImgfile(src_sf, files, Path.Combine(tileimgpath, basename + ".img"), overwrite);
      }

      /// <summary>
      /// kopiert die Dateien aus dem <see cref="SimpleFilesystem"/> in eine neu erzeugte IMG-Datei
      /// </summary>
      /// <param name="src_sf"><see cref="SimpleFilesystem"/> der Quell-IMG-Datei</param>
      /// <param name="files">Liste der Dateien</param>
      /// <param name="imgname">Name der neuen IMG-Datei</param>
      /// <param name="overwrite">true, wenn eine ev. schon ex. IMG-Datei überschrieben werden soll</param>
      void CreateNewImgfile(SimpleFilesystem src_sf, IList<string> files, string imgname, bool overwrite) {
         if (!overwrite && File.Exists(imgname))
            throw new Exception("Die Datei '" + imgname + "' existiert schon, darf aber nicht überschrieben werden.");

         using (SimpleFilesystem sfimg = new SimpleFilesystem()) {
            using (BinaryReaderWriter bw = new BinaryReaderWriter(File.OpenWrite(imgname))) {
               for (int i = 0; i < files.Count; i++) {
                  BinaryReaderWriter br = src_sf.GetBinaryReaderWriter4File(files[i]);
                  if (br != null) {
                     sfimg.FileAdd(files[i], (uint)br.Length);
                     Console.WriteLine(string.Format("füge '{0}' hinzu ...", files[i]));
                     using (BinaryReaderWriter bwf = sfimg.GetBinaryReaderWriter4File(files[i]))
                        br.CopyTo(bwf);
                     br.Dispose();
                  }
               }
               Console.WriteLine(string.Format("erzeuge '{0}' ...", imgname));
               sfimg.Write(bw);
               bw.Dispose();
            }
         }
      }

      /// <summary>
      /// erzeugt eine IMG-Datei und nimmt die Dateien der Liste in die IMG-Datei auf
      /// </summary>
      /// <param name="files"></param>
      /// <param name="imgname"></param>
      /// <param name="description"></param>
      /// <param name="overwrite"></param>
      void CreateNewImgfile(IList<string> files, string imgname, string description, bool overwrite) {
         if (!overwrite && File.Exists(imgname))
            throw new Exception("Die Datei '" + imgname + "' existiert schon, darf aber nicht überschrieben werden.");

         using (SimpleFilesystem sfimg = new SimpleFilesystem()) {
            using (BinaryReaderWriter bw = new BinaryReaderWriter(File.OpenWrite(imgname))) {
               for (int i = 0; i < files.Count; i++) {
                  BinaryReaderWriter br = new BinaryReaderWriter(files[i], true);
                  if (br != null) {
                     string newfilename = Path.GetFileName(files[i]);
                     sfimg.FileAdd(newfilename, (uint)br.Length);
                     Console.WriteLine(string.Format("füge '{0}' hinzu ...", files[i]));
                     using (BinaryReaderWriter bwf = sfimg.GetBinaryReaderWriter4File(newfilename))
                        br.CopyTo(bwf);
                     br.Dispose();
                  }
               }
               Console.Write(string.Format("erzeuge '{0}' ...", imgname));
               sfimg.ImgHeader.Description = description;
               sfimg.Write(bw);
               long len = bw.Length;
               bw.Dispose();
               Console.WriteLine(string.Format(" {0} Bytes geschrieben", len));

            }
         }

      }


      /// <summary>
      /// kopiert eine Datei aus dem <see cref="SimpleFilesystem"/> in eine neu erzeugte Datei
      /// </summary>
      /// <param name="src_sf"><see cref="SimpleFilesystem"/> der Quell-IMG-Datei</param>
      /// <param name="srcfilename">Name der Quell-Datei aus dem <see cref="SimpleFilesystem"/></param>
      /// <param name="destfilename">Name der neuen Datei</param>
      /// <param name="overwrite">true, wenn eine ev. schon ex. IMG-Datei überschrieben werden soll</param>
      static public void FileCopy(SimpleFilesystem src_sf, string srcfilename, string destfilename, bool overwrite) {
         if (!overwrite && File.Exists(destfilename))
            throw new Exception("Die Datei '" + destfilename + "' existiert schon, darf aber nicht überschrieben werden.");

         BinaryReaderWriter br = src_sf.GetBinaryReaderWriter4File(srcfilename);
         if (br != null) {
            if (!Directory.Exists(Path.GetDirectoryName(destfilename)))             // ev. muss das Zielverzeichnis erst erzeugt werden
               Directory.CreateDirectory(Path.GetDirectoryName(destfilename));
            using (BinaryReaderWriter bw = new BinaryReaderWriter(File.OpenWrite(destfilename))) {
               br.CopyTo(bw);
               bw.Dispose();
            }
            br.Dispose();
         }
      }

      /// <summary>
      /// liefert die Nummer zum Basisdateinamen (auch wenn ein Buchstabe am Anfang steht)
      /// </summary>
      /// <param name="basefilename"></param>
      /// <returns></returns>
      static public int GetNumber4Basefilename(string basefilename) {
         try {
            if (char.IsDigit(basefilename, 0) ||
                ('A' <= basefilename[0] && basefilename[0] <= 'F'))
               return int.Parse(basefilename, NumberStyles.HexNumber);
            return int.Parse(basefilename.Substring(1), NumberStyles.HexNumber);
         } catch { }
         return 0;
      }

      /// <summary>
      /// liefert einen 8stelligen Hex-Basisdateinamen zur Nummer
      /// </summary>
      /// <param name="no"></param>
      /// <param name="withprefix">wenn true, dann mit 'I' am Anfang</param>
      /// <returns></returns>
      static public string GetBasefilename4Number(int no, bool withprefix) {
         string txt = no.ToString("X8");
         if (withprefix)
            txt = 'I' + txt.Substring(1);
         return txt;
      }

      /// <summary>
      /// liefert die Basisdateinamen (aller TRE-Dateien)
      /// </summary>
      /// <param name="path"></param>
      /// <returns></returns>
      static public List<string> AllBasenames(string path) {
         return new List<string>(Directory.GetFiles(path, "*.TRE", SearchOption.TopDirectoryOnly));
      }

      /// <summary>
      /// liefert alle Dateinamen zu einem Basisdateinamen
      /// </summary>
      /// <param name="path"></param>
      /// <param name="basename"></param>
      /// <returns></returns>
      static public List<string> AllFilenames4Basename(string path, string basename) {
         return new List<string>(Directory.GetFiles(path, basename + ".*", SearchOption.TopDirectoryOnly));
      }

      #endregion
   }
}
