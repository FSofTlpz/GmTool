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
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GmTool {
   public class Info4File {

      static public void Info(string file, int info) {
         FileInfo fi = new FileInfo(file);
         if (!fi.Exists) {
            Console.WriteLine("Die Datei '" + file + "' existiert nicht.");
            return;
         }

         Console.WriteLine("Datei '" + file + "'");
         Console.WriteLine(string.Format("{0} Bytes ({1:F1} kB, {2:F1} MB), letzter Schreibzugriff {3} {4}",
                                         fi.Length,
                                         fi.Length / 1024.0,
                                         fi.Length / (1024.0 * 1024),
                                         fi.LastWriteTime.ToShortDateString(),
                                         fi.LastWriteTime.ToLongTimeString()));

         //using (BinaryReaderWriter br = new BinaryReaderWriter(File.OpenRead(file))) {
         using (BinaryReaderWriter brw = new BinaryReaderWriter(file, true)) {
            string ext = fi.Extension.ToUpper();
            StdFile_LBL lbl = new StdFile_LBL();
            StdFile_TRE tre = new StdFile_TRE();
            BinaryReaderWriter brlbl = null;
            BinaryReaderWriter brtre = null;

            if (ext == ".IMG") {

               Info_Specialfile(brw, ext, null, null, info);

               SimpleFilesystem sf = new SimpleFilesystem();
               sf.Read(brw);
               for (int i = 0; i < sf.FileCount; i++) {
                  Console.WriteLine("");
                  string subfile = sf.Filename(i);
                  Console.WriteLine("Sub-Datei '" + subfile + "'");
                  using (BinaryReaderWriter brf = sf.GetBinaryReaderWriter4File(subfile)) {
                     ext = subfile.Substring(subfile.Length - 4).ToUpper();
                     brlbl = null;

                     // für TRE, NET und RGN sind zusätzliche Dateien nötig
                     if (ext == ".TRE" ||
                         ext == ".NET") {
                        string basefilename = subfile.Substring(0, 8);
                        brlbl = sf.GetBinaryReaderWriter4File(basefilename + ".LBL");
                        if (brlbl != null)
                           lbl.Read(brlbl);
                     }
                     if (ext == ".RGN") {
                        string basefilename = subfile.Substring(0, 8);
                        brtre = sf.GetBinaryReaderWriter4File(basefilename + ".TRE");
                        if (brtre != null)
                           tre.Read(brtre);
                     }

                     Info_Specialfile(brf, ext, brlbl != null ? lbl : null, brtre != null ? tre : null, info);

                     if (brlbl != null)
                        brlbl.Dispose();
                     if (brtre != null)
                        brtre.Dispose();

                     brf.Dispose();
                  }
               }

            } else {
               // für TRE, NET und RGN sind zusätzliche Dateien nötig
               if (ext == ".TRE" ||
                   ext == ".NET") {
                  string lblfilename = file.Substring(0, file.Length - 4) + ".LBL";
                  if (File.Exists(lblfilename)) {
                     brlbl = new BinaryReaderWriter(lblfilename, true);
                     if (brlbl != null)
                        lbl.Read(brlbl);
                  }
               }
               if (ext == ".RGN") {
                  string trefilename = file.Substring(0, file.Length - 4) + ".TRE";
                  if (File.Exists(trefilename)) {
                     brtre = new BinaryReaderWriter(trefilename, true);
                     if (brtre != null)
                        tre.Read(brtre);
                  }
               }

               Info_Specialfile(brw, ext, brlbl != null ? lbl : null, brtre != null ? tre : null, info);

               if (brlbl != null)
                  brlbl.Dispose();
               if (brtre != null)
                  brtre.Dispose();
            }

            brw.Dispose();
         }
      }

      static void Info_Specialfile(BinaryReaderWriter br, string extension, StdFile_LBL lbl, StdFile_TRE tre, int info) {
         try {

            if (extension == ".IMG") {
               Info_IMG(br, info);
            } else if (extension == ".TRE") {
               Info_TRE(br, lbl, info);
            } else if (extension == ".LBL") {
               Info_LBL(br, info);
            } else if (extension == ".NET") {
               Info_NET(br, lbl, info);
            } else if (extension == ".NOD") {
               Info_NOD(br, info);
            } else if (extension == ".RGN") {
               Info_RGN(br, tre, info);
            } else if (extension == ".MDX") {
               Info_MDX(br, info);
            } else if (extension == ".TDB") {
               Info_TDB(br, info);
            } else if (extension == ".MDR") {
               Info_MDR(br, info);
            } else if (extension == ".TYP") {
               Info_TYP(br, info);
            } else if (extension == ".SRT") {
               Info_SRT(br, info);
            } else if (extension == ".MPS") {
               Info_MPS(br, info);
            } else if (extension == ".DEM") {
               Info_DEM(br, info);
            } else if (extension == ".MAR") {
               Info_MAR(br, info);
            } else if (extension == ".GMP") {
               Info_GMP(br, info);
            }

         } catch (Exception ex) {
            Console.Error.WriteLine("Fehler: " + ex.Message);
         }
      }

      #region Infos je Dateityp

      static void Info_IMG(BinaryReaderWriter br, int info) {
         SimpleFilesystem sf = new SimpleFilesystem();
         sf.Read(br);

         Info_ShowInfoItem(1, "Kartenbeschreibung: " + sf.ImgHeader.Description);
         Info_ShowInfoItem(1, "Zeitpunkt der Kartenerzeugung: " + sf.ImgHeader.CreationDate.ToShortDateString() + " " + sf.ImgHeader.CreationDate.ToLongTimeString());
         Info_ShowInfoItem(1, "Daten mit XOR: " + (sf.ImgHeader.XOR == 1).ToString());
         Info_ShowInfoItem(1, "Länge des Headers in Byte: " + sf.ImgHeader.HeaderLength.ToString() + " (0x" + sf.ImgHeader.HeaderLength.ToString("X") + ")");
         Info_ShowInfoItem(1, "Länge der FAT in Byte: " + sf.FATSize.ToString() + " (0x" + sf.FATSize.ToString("X") + ")");
         Info_ShowInfoItem(1, "Länge eines Datenblocks in Byte: " + sf.ImgHeader.FileBlockLength.ToString() + " (0x" + sf.ImgHeader.FileBlockLength.ToString("X") + ")");
         Info_ShowInfoItem(1, "Länge eines FAT-Datenblocks in Byte: " + sf.ImgHeader.FATBlockLength.ToString() + " (0x" + sf.ImgHeader.FileBlockLength.ToString("X") + ")");
         Info_ShowInfoItem(1, "enthält " + sf.FileCount.ToString() + " Dateien");
         if (info > 1) {
            Info_ShowInfoItem(1, "Unknown_x01", sf.ImgHeader.Unknown_x01, true);
            Info_ShowInfoItem(1, "Unknown_x0c", sf.ImgHeader.Unknown_x0c, true);
            Info_ShowInfoItem(1, "Unknown_x16", sf.ImgHeader.Unknown_x16, true);
            Info_ShowInfoItem(1, "Unknown_x1e", sf.ImgHeader.Unknown_x1e, true);
            Info_ShowInfoItem(1, "Unknown_x47", sf.ImgHeader.Unknown_x47, true);
            Info_ShowInfoItem(1, "Unknown_x83", sf.ImgHeader.Unknown_x83, true);
            Info_ShowInfoItem(1, "Unknown_x1ce", sf.ImgHeader.Unknown_x1ce, true);
            Info_ShowInfoItem(1, "Unknown_x200", sf.ImgHeader.Unknown_x200, true);
         }
         if (info > 0)
            for (int i = 0; i < sf.FileCount; i++) {
               uint flen = sf.Filesize(i);
               Info_ShowInfoItem(1, string.Format("Sub-Datei '{0}', {1} Bytes ({2:F1} kB, {3:F1} MB)",
                                                  sf.Filename(i),
                                                  flen,
                                                  flen / 1024.0,
                                                  flen / (1024.0 * 1024)));
            }



      }

      static void Info_TRE(uint firstlevel, StdFile_TRE tre, StdFile_LBL lbl, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", tre.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", tre.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", tre.Locked != 0);

         for (int i = 0; i < tre.MapDescriptionList.Count; i++) {
            string[] lines = tre.MapDescriptionList[i].Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < lines.Length; j++)
               Info_ShowInfoItem(firstlevel, i == 0 && j == 0 ? "Beschreibung" : "", lines[j]);
         }
         if (lbl != null) {
            for (int i = 0; i < tre.CopyrightOffsetsList.Count; i++) {
               string txt = lbl.GetText(tre.CopyrightOffsetsList[i]);
               if (txt != null) {
                  string[] lines = txt.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                  for (int j = 0; j < lines.Length; j++)
                     Info_ShowInfoItem(firstlevel, i == 0 && j == 0 ? "Copyright" : "", lines[j]);
               }
            }
         }
         Info_ShowInfoItem(firstlevel, "Anzeigebereich", string.Format("Lon {0:F6}°..{1:F6}° Lat {2:F6}°..{3:F6}°", tre.West.ValueDegree, tre.East.ValueDegree, tre.South.ValueDegree, tre.North.ValueDegree));
         Info_ShowInfoItem(firstlevel, "Kartenlayer", tre.DisplayPriority);
         Info_ShowInfoItem(firstlevel, "MapID", tre.MapID, true);
         if (!tre.RawRead) {
            Info_ShowInfoItem(firstlevel, "Subdiv's", tre.SubdivInfoList.Count);
            if (info > 1) {

               for (int i = 0; i < tre.SubdivInfoList.Count; i++) {
                  int level = tre.SymbolicScaleDenominatorAndBitsLevel.Level4SubdivIdx1(i + 1);
                  int coordbits = tre.SymbolicScaleDenominatorAndBitsLevel.Bits(level);
                  Info_ShowInfoItem(firstlevel + 1, string.Format("Nr. {0}, SymbolicScaleDenominator: {1}, Inhalt: {2}, letzte: {3}{4}, Lon {5:F6}°..{6:F6}° Lat {7:F6}°..{8:F6}°, {9}",
                                                                  i + 1,
                                                                  tre.SymbolicScaleDenominatorAndBitsLevel.SymbolicScaleDenominator(level),
                                                                  tre.SubdivInfoList[i].Content.ToString(),
                                                                  tre.SubdivInfoList[i].LastSubdiv,
                                                                  tre.SubdivInfoList[i] is StdFile_TRE.SubdivInfo ?
                                                                     string.Format(", Child {0}", (tre.SubdivInfoList[i] as StdFile_TRE.SubdivInfo).FirstChildSubdivIdx1) :
                                                                     "",
                                                                  tre.SubdivInfoList[i].Center.LongitudeDegree - tre.SubdivInfoList[i].GetHalfWidthDegree(coordbits),
                                                                  tre.SubdivInfoList[i].Center.LongitudeDegree + tre.SubdivInfoList[i].GetHalfWidthDegree(coordbits),
                                                                  tre.SubdivInfoList[i].Center.LatitudeDegree - tre.SubdivInfoList[i].GetHalfHeightDegree(coordbits),
                                                                  tre.SubdivInfoList[i].Center.LatitudeDegree + tre.SubdivInfoList[i].GetHalfHeightDegree(coordbits),
                                                                  tre.SubdivInfoList[i].Data));
               }
            }
            Info_ShowInfoItem(firstlevel, "Maplevel", tre.SymbolicScaleDenominatorAndBitsLevel.Count);
            if (info > 0) {
               List<StdFile_TRE.MapLevel> ml = tre.SymbolicScaleDenominatorAndBitsLevel.GetMaplevelList();
               for (int i = 0; i < ml.Count; i++)
                  Info_ShowInfoItem(firstlevel + 1, string.Format("SymbolicScaleDenominator {0}, {5} Bit je Koordinate, {6} Subdiv's (ab Nr. {7}), Inherited {1}, Bit 4,5,6 {2},{3},{4}",
                                                     ml[i].SymbolicScaleDenominator,
                                                     ml[i].Inherited,
                                                     ml[i].Bit4,
                                                     ml[i].Bit5,
                                                     ml[i].Bit6,
                                                     ml[i].CoordBits,
                                                     ml[i].SubdivInfos,
                                                     ml[i].FirstSubdivInfoNumber));
            }

            if (info > 1) {
               Info_ShowInfoItem(firstlevel, "Unknown_x3B", tre.Unknown_x3B, true);
               Info_ShowInfoItem(firstlevel, "Unknown_x43", tre.Unknown_x43, true);
               Info_ShowInfoItem(firstlevel, "Unknown_x54", tre.Unknown_x54, true);
               Info_ShowInfoItem(firstlevel, "Unknown_x62", tre.Unknown_x62, true);
               Info_ShowInfoItem(firstlevel, "Unknown_x70", tre.Unknown_x70, true);
               Info_ShowInfoItem(firstlevel, "Unknown_x78", tre.Unknown_x78, true);
               Info_ShowInfoItem(firstlevel, "Unknown_x86", tre.Unknown_x86, true);
               Info_ShowInfoItem(firstlevel, "Unknown_xB6", tre.Unknown_xB6, true);
               Info_ShowInfoItem(firstlevel, "Unknown_xC4", tre.Unknown_xC4, true);
               Info_ShowInfoItem(firstlevel, "Unknown_xEB", tre.Unknown_xEB, true);

               for (int ext = 0; ext < 2; ext++) {
                  bool exttype = ext > 0;
                  for (int t = 0; t < 3; t++) {
                     StdFile_TRE.Overview ovtype = (StdFile_TRE.Overview)t;
                     int max = tre.OverviewCount(ovtype, exttype);
                     if (max > 0) {
                        switch (ovtype) {
                           case StdFile_TRE.Overview.Point:
                              Info_ShowInfoItem(firstlevel, exttype ? "Overview ext. Points" : "Overview Points");
                              break;
                           case StdFile_TRE.Overview.Line:
                              Info_ShowInfoItem(firstlevel, exttype ? "Overview ext. Lines" : "Overview Lines");
                              break;
                           case StdFile_TRE.Overview.Area:
                              Info_ShowInfoItem(firstlevel, exttype ? "Overview ext. Areas" : "Overview Areas");
                              break;
                        }
                        for (int i = 0; i < max; i++) {
                           int maxlevel, type, subtype, unknown;
                           maxlevel = tre.GetOverviewData(ovtype, exttype, i, out type, out subtype, out unknown);
                           string txt = "Typ 0x";
                           if (type >= 0)
                              txt += type.ToString("x2");
                           if (subtype >= 0)
                              txt += subtype.ToString("x2");
                           if (unknown >= 0)
                              txt += ", unknown 0x" + unknown.ToString("x2");
                           if (maxlevel >= 0)
                              txt += ", MaxLevel " + maxlevel.ToString();
                           Info_ShowInfoItem(firstlevel + 1, "", txt);
                        }
                     }
                  }
               }
            }
         }
      }

      static void Info_TRE(BinaryReaderWriter br, StdFile_LBL lbl, int info) {
         StdFile_TRE tre = new StdFile_TRE();
         tre.Read(br);
         Info_TRE(1, tre, lbl, info);
      }

      static void Info_LBL(uint firstlevel, StdFile_LBL lbl, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", lbl.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", lbl.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", lbl.Locked != 0);
         Info_ShowInfoItem(firstlevel, "Codepage", lbl.Codepage, true);
         Info_ShowInfoItem(firstlevel, "Sortierung", lbl.SortDescriptor);
         Info_ShowInfoItem(firstlevel, "ID1", lbl.ID1, true);
         Info_ShowInfoItem(firstlevel, "ID2", lbl.ID2, true);
         if (!lbl.RawRead) {
            Info_ShowInfoItem(firstlevel, "Land-Einträge", lbl.CountryDataList.Count);
            Info_ShowInfoItem(firstlevel, "Region-Einträge", lbl.RegionAndCountryDataList.Count);
            Info_ShowInfoItem(firstlevel, "PLZ-Einträge", lbl.ZipDataList.Count);
            Info_ShowInfoItem(firstlevel, "Stadt-Einträge", lbl.CityAndRegionOrCountryDataList.Count);
            Info_ShowInfoItem(firstlevel, "Exit-Einträge", lbl.ExitList.Count);
            Info_ShowInfoItem(firstlevel, "Highway-Einträge", lbl.HighwayExitDefList.Count);
            Info_ShowInfoItem(firstlevel, "Index-POIS's", lbl.PoiIndexDataList.Count);
            Info_ShowInfoItem(firstlevel, "POI-Daten-Einträge", lbl.POIPropertiesList.Count);
            Info_ShowInfoItem(firstlevel, "Anzahl der Texte", lbl.TextList.Count);
         }
         if (info > 1) {

            Info_ShowInfoItem(firstlevel, "Unknown_0x29", lbl.Unknown_0x29, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x37", lbl.Unknown_0x37, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x45", lbl.Unknown_0x45, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x53", lbl.Unknown_0x53, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x61", lbl.Unknown_0x61, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x6E", lbl.Unknown_0x6E, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x7C", lbl.Unknown_0x7C, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x8A", lbl.Unknown_0x8A, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x98", lbl.Unknown_0x98, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0xA6", lbl.Unknown_0xA6, true);

            // --------- Headerlänge > 170 Byte
            if (lbl.Headerlength > 0xaa) {
               Info_ShowInfoItem(firstlevel, "Unknown_0xC2", lbl.Unknown_0xC2, true);
               if (lbl.Headerlength > 0xc2) {
                  Info_ShowInfoItem(firstlevel, "Unknown_0xCE", lbl.Unknown_0xCE, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0xD0 ", lbl.UnknownBlock_0xD0.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0xD8", lbl.Unknown_0xD8, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0xDE ", lbl.UnknownBlock_0xDE.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0xE6", lbl.Unknown_0xE6, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0xEC ", lbl.UnknownBlock_0xEC.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0xF4", lbl.Unknown_0xF4, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0xFA ", lbl.UnknownBlock_0xFA.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x102", lbl.Unknown_0x102, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x108", lbl.UnknownBlock_0x108.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x110", lbl.Unknown_0x110, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x116", lbl.UnknownBlock_0x116.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x11E", lbl.Unknown_0x11E, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x124", lbl.UnknownBlock_0x124.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x12C", lbl.Unknown_0x12C, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x132", lbl.UnknownBlock_0x132.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x13A", lbl.Unknown_0x13A, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x140", lbl.UnknownBlock_0x140.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x148", lbl.Unknown_0x148, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x14E", lbl.UnknownBlock_0x14E.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x156", lbl.Unknown_0x156, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x15A", lbl.UnknownBlock_0x15A.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x162", lbl.Unknown_0x162, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x168", lbl.UnknownBlock_0x168.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x170", lbl.Unknown_0x170, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x176", lbl.UnknownBlock_0x176.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x17E", lbl.Unknown_0x17E, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x184", lbl.UnknownBlock_0x184.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x18C", lbl.Unknown_0x18C, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x192", lbl.UnknownBlock_0x192.ToString());
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x19A", lbl.UnknownBlock_0x19A.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1A2", lbl.Unknown_0x1A2, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1A6", lbl.UnknownBlock_0x1A6.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1AE", lbl.Unknown_0x1AE, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1B2", lbl.UnknownBlock_0x1B2.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1BA", lbl.Unknown_0x1BA, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1BE", lbl.UnknownBlock_0x1BE.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1C6", lbl.Unknown_0x1C6, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1CA", lbl.UnknownBlock_0x1CA.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1D2", lbl.Unknown_0x1D2, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1D8", lbl.UnknownBlock_0x1D8.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1E0", lbl.Unknown_0x1E0, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1E6", lbl.UnknownBlock_0x1E6.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1EE", lbl.Unknown_0x1EE, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x1F2", lbl.UnknownBlock_0x1F2.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x1FA", lbl.Unknown_0x1FA, true);
                  Info_ShowInfoItem(firstlevel, "UnknownBlock_0x200", lbl.UnknownBlock_0x200.ToString());
                  Info_ShowInfoItem(firstlevel, "Unknown_0x208", lbl.Unknown_0x208, true);
               }
            }
         }
      }

      static void Info_LBL(BinaryReaderWriter br, int info) {
         StdFile_LBL lbl = new StdFile_LBL();
         lbl.Read(br);
         Info_LBL(1, lbl, info);
      }

      static void Info_NET(uint firstlevel, StdFile_NET net, StdFile_LBL lbl, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", net.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", net.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", net.Locked != 0);

         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "Unknown_0x31", net.Unknown_0x31, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x35", net.Unknown_0x35, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x36", net.Unknown_0x36, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x37", net.Unknown_0x37, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x3B", net.Unknown_0x3B, true);
            Info_ShowInfoItem(firstlevel, "UnknownBlock_0x43", net.UnknownBlock_0x43.ToString());
            Info_ShowInfoItem(firstlevel, "Unknown_0x4B", net.Unknown_0x4B, true);
            Info_ShowInfoItem(firstlevel, "UnknownBlock_0x4C", net.UnknownBlock_0x4C.ToString());
            Info_ShowInfoItem(firstlevel, "Unknown_0x54", net.Unknown_0x54, true);
            Info_ShowInfoItem(firstlevel, "UnknownBlock_0x56", net.UnknownBlock_0x56.ToString());
            Info_ShowInfoItem(firstlevel, "Unknown_0x5E", net.Unknown_0x5E, true);
         }

         if (!net.RawRead && net.Lbl != null && info > 0)
            Info_ShowInfoItem(firstlevel, "Straßen-Einträge", net.Roaddata.Count.ToString());
      }

      static void Info_NET(BinaryReaderWriter br, StdFile_LBL lbl, int info) {
         StdFile_NET net = new StdFile_NET();
         net.Lbl = lbl;
         net.Read(br, info == 0 ? false : true);
         Info_NET(1, net, lbl, info);
      }

      static void Info_NOD(uint firstlevel, StdFile_NOD nod, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", nod.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", nod.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", nod.Locked != 0);
         Info_ShowInfoItem(firstlevel, "Nod1", nod.Nod1);
         Info_ShowInfoItem(firstlevel, "Nod2", nod.Nod2);
         Info_ShowInfoItem(firstlevel, "Nod3", nod.Nod3);
         Info_ShowInfoItem(firstlevel, "Nod4", nod.Nod4);
         Info_ShowInfoItem(firstlevel, "Nod5", nod.Nod5);
         Info_ShowInfoItem(firstlevel, "Nod6", nod.Nod6);

         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "Unknown_0x1D", nod.Unknown_0x1D, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x1E", nod.Unknown_0x1E, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x1F", nod.Unknown_0x1F, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x21", nod.Unknown_0x21, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x23", nod.Unknown_0x23, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x2D", nod.Unknown_0x2D, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x3B", nod.Unknown_0x3B, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x47", nod.Unknown_0x47, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x6F", nod.Unknown_0x6F, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x6F", nod.Unknown_0x7B, true);
         }

      }

      static void Info_NOD(BinaryReaderWriter br, int info) {
         StdFile_NOD nod = new StdFile_NOD();
         nod.Read(br);
         Info_NOD(1, nod, info);
      }

      static void Info_RGN(uint firstlevel, StdFile_RGN rgn, StdFile_TRE tre, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", rgn.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", rgn.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", rgn.Locked != 0);
         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "UnknownBlock_0x71", rgn.UnknownBlock_0x71.ToString());
            Info_ShowInfoItem(firstlevel, "Unknown_0x25", rgn.Unknown_0x25, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x41", rgn.Unknown_0x41, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x5D", rgn.Unknown_0x5D, true);
            Info_ShowInfoItem(firstlevel, "Unknown_0x79", rgn.Unknown_0x79, true);
         }

         if (!rgn.RawRead && tre != null) {
            Info_ShowInfoItem(firstlevel, "Subdiv's", rgn.SubdivList.Count);
            int points = 0, idxpoints = 0, lines = 0, areas = 0, extpoints = 0, extlines = 0, extareas = 0;
            if (info > 0) {
               if (tre.SubdivInfoList.Count != rgn.SubdivList.Count)
                  Info_ShowInfoItem(firstlevel + 1, string.Format("Inkonsistenz bei der Subdiv-Anzahl zwischen TRE ({0}) und RGN ({1})!!!", tre.SubdivInfoList.Count, rgn.SubdivList.Count));
               for (int i = 0; i < tre.SubdivInfoList.Count && i < rgn.SubdivList.Count; i++) {
                  int level = tre.SymbolicScaleDenominatorAndBitsLevel.Level4SubdivIdx1(i + 1);
                  int coordbits = tre.SymbolicScaleDenominatorAndBitsLevel.Bits(level);

                  StdFile_TRE.SubdivInfoBasic sdinf = tre.SubdivInfoList[i];
                  StdFile_RGN.SubdivData sd = rgn.SubdivList[i];
                  idxpoints += sd.IdxPointList.Count;
                  points += sd.PointList.Count;
                  lines += sd.LineList.Count;
                  areas += sd.AreaList.Count;
                  extpoints += sd.ExtPointList.Count;
                  extlines += sd.ExtLineList.Count;
                  extareas += sd.ExtAreaList.Count;

                  if (info > 1) {
                     foreach (var item in sd.IdxPointList) {
                        MapUnitPoint pt = item.GetMapUnitPoint(coordbits, sdinf.Center);
                        Info_ShowInfoItem(firstlevel + 1, string.Format("Subdiv {0}, IDX-Punkt 0x{1:x2}{2:x2}, Label: {3}, ({4:G}° {5:G}°)",
                                                            i + 1,
                                                            item.Typ,
                                                            item.Subtyp,
                                                            item.LabelOffset > 0,
                                                            pt.LongitudeDegree,
                                                            pt.LatitudeDegree));
                     }
                     foreach (var item in sd.PointList) {
                        MapUnitPoint pt = item.GetMapUnitPoint(coordbits, sdinf.Center);
                        Info_ShowInfoItem(firstlevel + 1, string.Format("Subdiv {0}, Punkt 0x{1:x2}{2:x2}, Label: {3}, ({4:G}° {5:G}°)",
                                                            i + 1,
                                                            item.Typ,
                                                            item.Subtyp,
                                                            item.LabelOffset > 0,
                                                            pt.LongitudeDegree,
                                                            pt.LatitudeDegree));
                     }
                     foreach (var item in sd.LineList) {
                        List<MapUnitPoint> pt = item.GetMapUnitPoints(coordbits, sdinf.Center);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Subdiv {0}, Linie 0x{1:x2}, Direction: {2}, Label: {3}, {4} Punkte",
                                                            i + 1,
                                                            item.Typ,
                                                            item.DirectionIndicator,
                                                            item.LabelOffset > 0,
                                                            pt.Count);
                        for (int j = 0; j < pt.Count; j++)
                           sb.AppendFormat(", {0}", pt[j].ToString());
                        Info_ShowInfoItem(firstlevel + 1, sb.ToString());
                     }
                     foreach (var item in sd.AreaList) {
                        List<MapUnitPoint> pt = item.GetMapUnitPoints(coordbits, sdinf.Center);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Subdiv {0}, Fläche 0x{1:x2}, Direction: {2}, Label: {3}, {4} Punkte",
                                                            i + 1,
                                                            item.Typ,
                                                            item.DirectionIndicator,
                                                            item.LabelOffset > 0,
                                                            pt.Count);
                        for (int j = 0; j < pt.Count; j++)
                           sb.AppendFormat(", {0}", pt[j].ToString());
                        Info_ShowInfoItem(firstlevel + 1, sb.ToString());
                     }

                     foreach (var item in sd.ExtPointList) {
                        MapUnitPoint pt = item.GetMapUnitPoint(coordbits, sdinf.Center);
                        Info_ShowInfoItem(firstlevel + 1, string.Format("Subdiv {0}, erw. Punkt 0x{1:x2}{2:x2}, Label: {3}, ({4:G}° {5:G}°)",
                                                            i + 1,
                                                            item.Typ,
                                                            item.Subtyp,
                                                            item.HasLabel,
                                                            pt.Longitude,
                                                            pt.Latitude));
                     }
                     foreach (var item in sd.ExtLineList) {
                        List<MapUnitPoint> pt = item.GetMapUnitPoints(coordbits, sdinf.Center);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Subdiv {0}, erw. Linie 0x{1:x2}{2:x2}, Label: {3}, {4} Punkte",
                                                            i + 1,
                                                            item.Typ,
                                                            item.Subtyp,
                                                            item.HasLabel,
                                                            pt.Count);
                        for (int j = 0; j < pt.Count; j++)
                           sb.AppendFormat(", {0}", pt[j].ToString());
                        Info_ShowInfoItem(firstlevel + 1, sb.ToString());
                     }
                     foreach (var item in sd.ExtAreaList) {
                        List<MapUnitPoint> pt = item.GetMapUnitPoints(coordbits, sdinf.Center);
                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("Subdiv {0}, erw. Fläche 0x{1:x2}{2:x2}, Label: {3}, {4} Punkte",
                                                            i + 1,
                                                            item.Typ,
                                                            item.Subtyp,
                                                            item.HasLabel,
                                                            pt.Count);
                        for (int j = 0; j < pt.Count; j++)
                           sb.AppendFormat(", {0}", pt[j].ToString());
                        Info_ShowInfoItem(firstlevel + 1, sb.ToString());
                     }
                  }
               }
            }
            Info_ShowInfoItem(firstlevel + 1, "IDX-Punkte", idxpoints);
            Info_ShowInfoItem(firstlevel + 1, "Punkte", points);
            Info_ShowInfoItem(firstlevel + 1, "Linien", lines);
            Info_ShowInfoItem(firstlevel + 1, "Flächen", areas);
            Info_ShowInfoItem(firstlevel + 1, "erw. Punkte", extpoints);
            Info_ShowInfoItem(firstlevel + 1, "erw. Linien", extlines);
            Info_ShowInfoItem(firstlevel + 1, "erw. Flächen", extareas);
         }
      }

      static void Info_RGN(BinaryReaderWriter br, StdFile_TRE tre, int info) {
         StdFile_RGN rgn = new StdFile_RGN(tre);
         rgn.Read(br);
         Info_RGN(1, rgn, tre, info);
      }

      static void Info_TYP(uint firstlevel, StdFile_TYP typ, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", typ.CreationDate);
         Info_ShowInfoItem(firstlevel, "HeaderTyp", typ.HeaderTyp.ToString());
         Info_ShowInfoItem(firstlevel, "Headerlength", typ.Headerlength, true);
         Info_ShowInfoItem(firstlevel, "Codepage", typ.Codepage, true);
         Info_ShowInfoItem(firstlevel, "Family-ID", typ.FamilyID, true);
         Info_ShowInfoItem(firstlevel, "Product-ID", typ.ProductID, true);

         Info_ShowInfoItem(firstlevel, "Punktdefinitionen", typ.PoiCount);
         StringBuilder sb = new StringBuilder();
         if (info > 0)
            for (int i = 0; i < typ.PoiCount; i++) {
               sb.Clear();
               GarminCore.Files.Typ.POI poi = typ.GetPoi(i);
               sb.Append("0x" + (((int)(poi.Typ) << 8) + (int)poi.Subtyp).ToString("X"));
               string txt = poi.Text.GetAsSimpleString();
               if (txt.Length > 0)
                  sb.Append(", " + txt);
               sb.AppendFormat(", Bitmap {0}x{1}, Colormode {2} / {3}", poi.Width, poi.Height, poi.ColormodeDay, poi.ColormodeNight);
               Info_ShowInfoItem(firstlevel + 1, "", sb.ToString());
            }

         Info_ShowInfoItem(firstlevel, "Liniendefinitionen", typ.PolylineCount);
         if (info > 0)
            for (int i = 0; i < typ.PolylineCount; i++) {
               sb.Clear();
               GarminCore.Files.Typ.Polyline poly = typ.GetPolyline(i);
               sb.Append("0x" + (((int)(poly.Typ) << 8) + (int)poly.Subtyp).ToString("X"));
               string txt = poly.Text.GetAsSimpleString();
               if (txt.Length > 0)
                  sb.Append(", " + txt);
               sb.AppendFormat(", Polylinetype {0}, Height {1}, InnerWidth {2}, BorderWidth {3}, BitmapHeight {4}", poly.Polylinetype, poly.Height, poly.InnerWidth, poly.BorderWidth, poly.BitmapHeight);
               Info_ShowInfoItem(firstlevel + 1, "", sb.ToString());
            }

         Info_ShowInfoItem(firstlevel, "Gebietsdefinitionen", typ.PolygonCount);
         if (info > 0)
            for (int i = 0; i < typ.PolygonCount; i++) {
               sb.Clear();
               GarminCore.Files.Typ.Polygone poly = typ.GetPolygone(i);
               sb.Append("0x" + (((int)(poly.Typ) << 8) + (int)poly.Subtyp).ToString("X"));
               string txt = poly.Text.GetAsSimpleString();
               if (txt.Length > 0)
                  sb.Append(", " + txt);
               sb.AppendFormat(", Polygontype {0}, Draworder {1}, DayColor1 {2}, DayColor2 {3}", poly.Polygontype, poly.Draworder, poly.DayColor1.ToString(), poly.DayColor2.ToString());
               Info_ShowInfoItem(firstlevel + 1, "", sb.ToString());
            }
      }

      static void Info_TYP(BinaryReaderWriter br, int info) {
         StdFile_TYP typ = new StdFile_TYP();
         typ.Read(br, true);
         Info_TYP(1, typ, info);
      }

      static void Info_TDB(uint firstlevel, File_TDB tdb, int info) {
         Info_ShowInfoItem(firstlevel, "TDBVersion", tdb.Head.TDBVersion);
         Info_ShowInfoItem(firstlevel, "CodePage", tdb.Head.CodePage, true);
         Info_ShowInfoItem(firstlevel, "Family-ID", tdb.Head.FamilyID, true);
         Info_ShowInfoItem(firstlevel, "Product-ID", tdb.Head.ProductID, true);
         Info_ShowInfoItem(firstlevel, "ProductVersion", tdb.Head.ProductVersion, true);
         Info_ShowInfoItem(firstlevel, "FamilyName", tdb.Head.MapFamilyName);
         Info_ShowInfoItem(firstlevel, "SeriesName", tdb.Head.MapSeriesName);
         Info_ShowInfoItem(firstlevel, "Routingfähig", tdb.Head.Routable, true);
         Info_ShowInfoItem(firstlevel, "größter Routing-Typ", tdb.Head.HighestRoutable, true);
         Info_ShowInfoItem(firstlevel, "mit Contourlinien", tdb.Head.HasProfileInformation, true);
         Info_ShowInfoItem(firstlevel, "DEM", tdb.Head.HasDEM, true);
         Info_ShowInfoItem(firstlevel, "niedrigster MapLevel", tdb.Head.MaxCoordbits4Overview, true);
         Info_ShowInfoItem(firstlevel, "Beschreibung", tdb.Mapdescription.Text);
         if (info > 1) {
            if (tdb.Head.TDBVersion >= 407) {   // 41 weitere Byte, z. T. unbekannte Daten für Headertyp >= 4.07

               Info_ShowInfoItem(firstlevel, "Unknown1", tdb.Head.Unknown1, true);
               Info_ShowInfoItem(firstlevel, "Unknown2", tdb.Head.Unknown2, true);
               Info_ShowInfoItem(firstlevel, "Unknown3", tdb.Head.Unknown3, true);
               Info_ShowInfoItem(firstlevel, "Unknown4", tdb.Head.Unknown4, true);
               Info_ShowInfoItem(firstlevel, "Unknown5", tdb.Head.Unknown5, true);
               Info_ShowInfoItem(firstlevel, "Unknown6", tdb.Head.Unknown6, true);
               Info_ShowInfoItem(firstlevel, "Unknown7", tdb.Head.Unknown7, true);
               Info_ShowInfoItem(firstlevel, "Unknown8", tdb.Head.Unknown8, true);
               Info_ShowInfoItem(firstlevel, "Unknown9", tdb.Head.Unknown9, true);
               Info_ShowInfoItem(firstlevel, "Unknown10", tdb.Head.Unknown10, true);
               Info_ShowInfoItem(firstlevel, "Unknown11", tdb.Head.Unknown11, true);
               Info_ShowInfoItem(firstlevel, "Unknown12", tdb.Head.Unknown12, true);
               Info_ShowInfoItem(firstlevel, "Unknown13", tdb.Head.Unknown13, true);
               Info_ShowInfoItem(firstlevel, "Unknown14", tdb.Head.Unknown14, true);
               Info_ShowInfoItem(firstlevel, "Unknown15", tdb.Head.Unknown15, true);
               Info_ShowInfoItem(firstlevel, "Unknown16", tdb.Head.Unknown16, true);
               Info_ShowInfoItem(firstlevel, "Unknown17", tdb.Head.Unknown17, true);
               Info_ShowInfoItem(firstlevel, "Unknown18", tdb.Head.Unknown18, true);
               Info_ShowInfoItem(firstlevel, "Unknown19", tdb.Head.Unknown19, true);
               Info_ShowInfoItem(firstlevel, "Unknown20", tdb.Head.Unknown20, true);
               Info_ShowInfoItem(firstlevel, "Unknown21", tdb.Head.Unknown21, true);
               Info_ShowInfoItem(firstlevel, "Unknown22", tdb.Head.Unknown22, true);
               Info_ShowInfoItem(firstlevel, "Unknown23", tdb.Head.Unknown23, true);
               Info_ShowInfoItem(firstlevel, "Unknown24", tdb.Head.Unknown24, true);
               Info_ShowInfoItem(firstlevel, "Unknown25", tdb.Head.Unknown25, true);
               Info_ShowInfoItem(firstlevel, "Unknown26", tdb.Head.Unknown26, true);
               Info_ShowInfoItem(firstlevel, "Unknown27", tdb.Head.Unknown27, true);
               Info_ShowInfoItem(firstlevel, "Unknown28", tdb.Head.Unknown28, true);
               Info_ShowInfoItem(firstlevel, "Unknown29", tdb.Head.Unknown29, true); // UInt32

               if (tdb.Head.TDBVersion >= 411) {   // 20 weitere Byte für Headertyp >= 4.11

                  Info_ShowInfoItem(firstlevel, "Unknown30", tdb.Head.Unknown30, true);
                  Info_ShowInfoItem(firstlevel, "Unknown31", tdb.Head.Unknown31, true);
                  Info_ShowInfoItem(firstlevel, "Unknown32", tdb.Head.Unknown32, true);
                  Info_ShowInfoItem(firstlevel, "Unknown33", tdb.Head.Unknown33, true);
                  Info_ShowInfoItem(firstlevel, "Unknown34", tdb.Head.Unknown34, true);
                  Info_ShowInfoItem(firstlevel, "Unknown35", tdb.Head.Unknown35, true);
                  Info_ShowInfoItem(firstlevel, "Unknown36", tdb.Head.Unknown36, true);
                  Info_ShowInfoItem(firstlevel, "Unknown37", tdb.Head.Unknown37, true);
                  Info_ShowInfoItem(firstlevel, "Unknown38", tdb.Head.Unknown38, true);
                  Info_ShowInfoItem(firstlevel, "Unknown39", tdb.Head.Unknown39, true);
                  Info_ShowInfoItem(firstlevel, "Unknown40", tdb.Head.Unknown40, true);
                  Info_ShowInfoItem(firstlevel, "Unknown41", tdb.Head.Unknown41, true);
                  Info_ShowInfoItem(firstlevel, "Unknown42", tdb.Head.Unknown42, true);
                  Info_ShowInfoItem(firstlevel, "Unknown43", tdb.Head.Unknown43, true);
                  Info_ShowInfoItem(firstlevel, "Unknown44", tdb.Head.Unknown44, true);
                  Info_ShowInfoItem(firstlevel, "Unknown45", tdb.Head.Unknown45, true);
                  Info_ShowInfoItem(firstlevel, "Unknown46", tdb.Head.Unknown46, true);
                  Info_ShowInfoItem(firstlevel, "Unknown47", tdb.Head.Unknown47, true);
                  Info_ShowInfoItem(firstlevel, "Unknown48", tdb.Head.Unknown48, true);
                  Info_ShowInfoItem(firstlevel, "Unknown49", tdb.Head.Unknown49, true);

               }
            }
         }

         for (int i = 0; i < tdb.Copyright.Segments.Count; i++) {
            Info_ShowInfoItem(firstlevel, i == 0 ? "Copyright" : "", "[" + tdb.Copyright.Segments[i].CopyrightCode.ToString() + ", " + tdb.Copyright.Segments[i].WhereCode.ToString() + "]");
            string[] lines = tdb.Copyright.Segments[i].Copyright.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < lines.Length; j++)
               Info_ShowInfoItem(firstlevel, "", lines[j]);
         }


         Info_ShowInfoItem(firstlevel, "Overviewmap", "");
         Info_ShowInfoItem(firstlevel + 1, "Kartenummer", tdb.Overviewmap.Mapnumber, true);
         Info_ShowInfoItem(firstlevel + 1, "übergeordnet", tdb.Overviewmap.ParentMapnumber, true);
         Info_ShowInfoItem(firstlevel + 1, "Beschreibung", tdb.Overviewmap.Description);
         Info_ShowInfoItem(firstlevel + 1, "Anzeigebereich", string.Format("Lon {0:F6}°..{1:F6}°, Lat {2:F6}°..{3:F6}°",
                                                              tdb.Overviewmap.West,
                                                              tdb.Overviewmap.East,
                                                              tdb.Overviewmap.South,
                                                              tdb.Overviewmap.North));

         Info_ShowInfoItem(firstlevel, "Detailkarten", tdb.Tilemap.Count);
         if (info > 0) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < tdb.Tilemap.Count; i++) {
               File_TDB.TileMap tm = tdb.Tilemap[i];
               Info_ShowInfoItem(firstlevel + 1, "Kartenummer", tm.Mapnumber, true);
               Info_ShowInfoItem(firstlevel + 2, "übergeordnet", tm.ParentMapnumber, true);
               Info_ShowInfoItem(firstlevel + 2, "Beschreibung", tm.Description);
               Info_ShowInfoItem(firstlevel + 2, "Anzeigebereich", string.Format("Lon {0:F6}°..{1:F6}°, Lat {2:F6}°..{3:F6}°", tm.West, tm.East, tm.South, tm.North));
               sb.Clear();
               for (int j = 0; j < tm.SubCount; j++) {
                  if (j > 0)
                     sb.Append(", ");
                  sb.Append(tm.Name[j]);
                  if (info > 1)
                     sb.AppendFormat(" ({0} Byte)", tm.DataSize[j]);
               }
               Info_ShowInfoItem(firstlevel + 2, "Dateien", sb.ToString());
               if (info > 1) {
                  Info_ShowInfoItem(firstlevel + 2, "HasCopyright", tm.HasCopyright, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown1", tm.Unknown1, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown2", tm.Unknown2, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown3", tm.Unknown3, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown4", tm.Unknown4, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown5", tm.Unknown5, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown6", tm.Unknown6, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown7", tm.Unknown7, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown8", tm.Unknown8, true);
                  Info_ShowInfoItem(firstlevel + 2, "Unknown9", tm.Unknown9, true);
               }
            }
         }

         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "CRC", tdb.Crc.GetBytes(), true);
            Info_ShowInfoItem(firstlevel + 1, "crc", tdb.Crc.crc, true);
         }

      }

      static void Info_TDB(BinaryReaderWriter br, int info) {
         File_TDB tdb = new File_TDB();
         tdb.Read(br);
         Info_TDB(1, tdb, info);
      }

      static void Info_SRT(uint firstlevel, StdFile_SRT srt, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", srt.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", srt.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", srt.Locked != 0);
         Info_ShowInfoItem(firstlevel, "Beschreibung", srt.Description);
         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "Unknown_x15", srt.Unknown_x15, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x1D", srt.Unknown_x1D, true);
         }
         Info_ShowInfoItem(firstlevel, "Sortheader.Codepage", srt.Sortheader.Codepage, true);
         Info_ShowInfoItem(firstlevel, "Sortheader.ID1", srt.Sortheader.Id1, true);
         Info_ShowInfoItem(firstlevel, "Sortheader.ID2", srt.Sortheader.Id2, true);
         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "Sortheader.Unknown1", srt.Sortheader.Unknown1, true);
            Info_ShowInfoItem(firstlevel, "Sortheader.Unknown2", srt.Sortheader.Unknown2, true);
            Info_ShowInfoItem(firstlevel, "Sortheader.Unknown3", srt.Sortheader.Unknown3, true);
            Info_ShowInfoItem(firstlevel, "Sortheader.Unknown4", srt.Sortheader.Unknown4, true);
            Info_ShowInfoItem(firstlevel, "Sortheader.Unknown5", srt.Sortheader.Unknown5, true);
            Info_ShowInfoItem(firstlevel, "Sortheader.Unknown6", srt.Sortheader.Unknown6, true);
         }
      }

      static void Info_SRT(BinaryReaderWriter br, int info) {
         StdFile_SRT srt = new StdFile_SRT();
         srt.Read(br);
         Info_SRT(1, srt, info);
      }

      static void Info_MDR(uint firstlevel, StdFile_MDR mdr, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", mdr.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", mdr.Headerlength);
         Info_ShowInfoItem(firstlevel, "gesperrt", mdr.Locked != 0);
         Info_ShowInfoItem(firstlevel, "Codepage", mdr.Codepage, true);
         Info_ShowInfoItem(firstlevel, "SortId1", mdr.SortId1, true);
         Info_ShowInfoItem(firstlevel, "SortId2", mdr.SortId2, true);
         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "Unknown_x1B", mdr.Unknown_x1B, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x27", mdr.Unknown_x27, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x35", mdr.Unknown_x35, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x43", mdr.Unknown_x43, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x51", mdr.Unknown_x51, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x5F", mdr.Unknown_x5F, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x6D", mdr.Unknown_x6D, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x7B", mdr.Unknown_x7B, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x89", mdr.Unknown_x89, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x97", mdr.Unknown_x97, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xA3", mdr.Unknown_xA3, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xB1", mdr.Unknown_xB1, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xBF", mdr.Unknown_xBF, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xCD", mdr.Unknown_xCD, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xDB", mdr.Unknown_xDB, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xE7", mdr.Unknown_xE7, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xF2", mdr.Unknown_xF2, true);
            Info_ShowInfoItem(firstlevel, "Unknown_xFE", mdr.Unknown_xFE, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x10C", mdr.Unknown_x10C, true);
            Info_ShowInfoItem(firstlevel, "Unknown_x110", mdr.Unknown_x110, true);
         }
      }

      static void Info_MDR(BinaryReaderWriter br, int info) {
         StdFile_MDR mdr = new StdFile_MDR();
         mdr.Read(br);
         Info_MDR(1, mdr, info);
      }

      static void Info_MDX(uint firstlevel, File_MDX mdx, int info) {
         Info_ShowInfoItem(firstlevel, "Kartenanzahl", mdx.Maps.Count);
         if (info > 1) {
            Info_ShowInfoItem(firstlevel, "Unknown1", mdx.Unknown1, true);
            Info_ShowInfoItem(firstlevel, "Unknown2", mdx.Unknown2, true);
         }
         foreach (File_MDX.MapEntry me in mdx.Maps) {
            Info_ShowInfoItem(firstlevel, "MapNumber", me.MapNumber, true);
            Info_ShowInfoItem(firstlevel + 1, "MapID", me.MapID, true);
            Info_ShowInfoItem(firstlevel + 1, "ProductID", me.ProductID, true);
            Info_ShowInfoItem(firstlevel + 1, "FamilyID", me.FamilyID, true);
         }
      }

      static void Info_MDX(BinaryReaderWriter br, int info) {
         File_MDX mdx = new File_MDX();
         mdx.Read(br);
         Info_MDX(1, mdx, info);
      }

      static void Info_MPS(uint firstlevel, File_MPS mps, int info) {
         Info_ShowInfoItem(firstlevel, "Kartenanzahl", mps.Maps.Count);
         foreach (File_MPS.MapEntry me in mps.Maps) {
            Info_ShowInfoItem(firstlevel, "MapNumber", me.MapNumber, true);
            Info_ShowInfoItem(firstlevel + 1, "Typ/Name", me.Typ.ToString() + ": " + string.Join("; ", me.Name));
            Info_ShowInfoItem(firstlevel + 1, "ProductID", me.ProductID, true);
            Info_ShowInfoItem(firstlevel + 1, "FamilyID", me.FamilyID, true);

            if (info > 1) {
               Info_ShowInfoItem(firstlevel + 1, "Unknown0", me.Unknown0, true);
               Info_ShowInfoItem(firstlevel + 1, "Unknown1", me.Unknown1, true);
               Info_ShowInfoItem(firstlevel + 1, "Unknown2", me.Unknown2, true);
               Info_ShowInfoItem(firstlevel + 1, "Unknown3", me.Unknown3, true);
               Info_ShowInfoItem(firstlevel + 1, "Unknown4", me.Unknown4, true);
            }

         }
      }

      static void Info_MPS(BinaryReaderWriter br, int info) {
         File_MPS mps = new File_MPS();
         mps.Read(br);
         Info_MPS(1, mps, info);
      }

      static void Info_DEM(uint firstlevel, StdFile_DEM dem, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", dem.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", dem.Headerlength);
         Info_ShowInfoItem(firstlevel, "Höhen in Metern", !dem.HeigthInFeet);
         if (info > 0) {

            if (info > 1) {
               Info_ShowInfoItem(firstlevel, "Unknown_0x0C", dem.Unknown_0x0C, true);
               Info_ShowInfoItem(firstlevel, "Unknown_0x1B", dem.Unknown_0x1B, true);
               Info_ShowInfoItem(firstlevel, "Unknown_0x25", dem.Unknown_0x25, true);
            }

            for (int z = 0; z < dem.ZoomLevel.Count; z++) {
               GarminCore.Files.DEM.ZoomlevelTableitem zl = dem.ZoomLevel[z].ZoomlevelItem;
               Info_ShowInfoItem(firstlevel, "Datenbereich", (z + 1).ToString() + ", Level " + zl.No + ", SpecType " + zl.SpecType.ToString());
               Info_ShowInfoItem(firstlevel + 1, "Ecke links-oben", zl.West.ToString() + "° / " + zl.North.ToString() + "°");
               Info_ShowInfoItem(firstlevel + 1, "Höhenbereich", zl.MinHeight.ToString() + " .. " + (zl.MaxHeight).ToString());
               Info_ShowInfoItem(firstlevel + 1, "Kacheln", (zl.MaxIdxHoriz + 1).ToString() + " x " + (zl.MaxIdxVert + 1).ToString() + " = " + ((zl.MaxIdxHoriz + 1) * (zl.MaxIdxVert + 1)).ToString());
               Info_ShowInfoItem(firstlevel + 1, "Kachelgröße", zl.PointsHoriz.ToString() + " x " + zl.PointsVert.ToString() + " Pixel, " +
                                                               (zl.PointsHoriz * zl.PointDistanceHoriz).ToString() + "° / " + (zl.PointsVert * zl.PointDistanceVert).ToString() + "°");
               Info_ShowInfoItem(firstlevel + 1, "Pixelgröße", GarminCore.Files.DEM.ZoomlevelTableitem.Degree2Unit(zl.PointDistanceHoriz).ToString() + " x " +
                                                               GarminCore.Files.DEM.ZoomlevelTableitem.Degree2Unit(zl.PointDistanceVert).ToString() + ", " +
                                                               zl.PointDistanceHoriz.ToString() + "° x " + zl.PointDistanceVert.ToString() + "°");
               Info_ShowInfoItem(firstlevel + 1, "Br letzte K.spalte", zl.LastColWidth + 1);
               Info_ShowInfoItem(firstlevel + 1, "Hö letzte K.zeile", zl.LastRowHeight + 1);
               Info_ShowInfoItem(firstlevel + 1, "Kacheltabelle", zl.PtrSubtileTable, true);
               Info_ShowInfoItem(firstlevel + 1, "Kacheltab.länge", zl.PtrHeightdata - zl.PtrSubtileTable, true);
               Info_ShowInfoItem(firstlevel + 1, "Kachelsatzlänge", zl.SubtileTableitemSize);
               Info_ShowInfoItem(firstlevel + 2, "Feldgrößen", "Offs. " + zl.Structure_OffsetSize.ToString() +
                                                               ", Basis " + zl.Structure_BaseheightSize.ToString() +
                                                               ", Diff. " + zl.Structure_DiffSize.ToString() +
                                                               ", Typ " + zl.Structure_CodingtypeSize.ToString());
               Info_ShowInfoItem(firstlevel + 1, "Datenbereichlänge", (z < dem.ZoomLevel.Count - 1 ? dem.ZoomLevel[z + 1].ZoomlevelItem.PtrSubtileTable : dem.PtrZoomlevel) - zl.PtrHeightdata, true);

               if (info > 2) {
                  for (int j = 0; j < dem.ZoomLevel[z].Subtiles.Count; j++) {
                     Info_ShowInfoItem(firstlevel + 1, "Datensatz", (j + 1).ToString());

                     StdFile_DEM.Subtile t = dem.ZoomLevel[z].Subtiles[j];
                     Info_ShowInfoItem(firstlevel + 2, "Adresse", zl.PtrSubtileTable + (uint)(j * zl.SubtileTableitemSize), true);
                     Info_ShowInfoItem(firstlevel + 2, "Basishöhe", t.Tableitem.Baseheight);
                     Info_ShowInfoItem(firstlevel + 2, "max. Diff.", t.Tableitem.Diff);
                     Info_ShowInfoItem(firstlevel + 2, "Typ", t.Tableitem.Type);
                     Info_ShowInfoItem(firstlevel + 2, "Datenlänge", t.DataLength, true);
                     Info_ShowInfoItem(firstlevel + 2, "Daten", t.Tableitem.Offset + zl.PtrHeightdata, true);

                  }
               }
            }
         }
      }

      static void Info_DEM(BinaryReaderWriter br, int info) {
         StdFile_DEM dem = new StdFile_DEM();
         dem.Read(br);
         Info_DEM(1, dem, info);
      }

      static void Info_MAR(uint firstlevel, StdFile_MAR mar, int info) {
         Info_ShowInfoItem(firstlevel, "Dateinhalt ist völlig unbekannt");
      }

      static void Info_MAR(BinaryReaderWriter br, int info) {
         StdFile_MAR mar = new StdFile_MAR();
         mar.Read(br);
         Info_MAR(1, mar, info);
      }

      static void Info_GMP(uint firstlevel, StdFile_GMP gmp, int info) {
         Info_ShowInfoItem(firstlevel, "Dateierzeugung", gmp.CreationDate);
         Info_ShowInfoItem(firstlevel, "Headerlänge", gmp.Headerlength);
         for (int j = 0; j < gmp.Copyright.Count; j++)
            Info_ShowInfoItem(firstlevel, j == 0 ? "gmp.Copyright" : "", gmp.Copyright[j]);

         if (gmp.TRE != null) {
            Info_ShowInfoItem(firstlevel, "enthält TRE-Daten");
            if (info > 0)
               Info_TRE(firstlevel + 1, gmp.TRE, gmp.LBL, info);
         }
         if (gmp.RGN != null) {
            Info_ShowInfoItem(firstlevel, "enthält RGN-Daten");
            if (info > 0)
               Info_RGN(firstlevel + 1, gmp.RGN, gmp.TRE, info);
         }
         if (gmp.LBL != null) {
            Info_ShowInfoItem(firstlevel, "enthält LBL-Daten");
            if (info > 0)
               Info_LBL(firstlevel + 1, gmp.LBL, info);
         }
         if (gmp.NET != null) {
            Info_ShowInfoItem(firstlevel, "enthält NET-Daten");
            if (info > 0)
               Info_NET(firstlevel + 1, gmp.NET, gmp.LBL, info);
         }
         if (gmp.NOD != null) {
            Info_ShowInfoItem(firstlevel, "enthält NOD-Daten");
            if (info > 0)
               Info_NOD(firstlevel + 1, gmp.NOD, info);
         }
         if (gmp.DEM != null) {
            Info_ShowInfoItem(firstlevel, "enthält DEM-Daten");
            if (info > 0)
               Info_DEM(firstlevel + 1, gmp.DEM, info);
         }
         if (gmp.MAR != null) {
            Info_ShowInfoItem(firstlevel, "enthält MAR-Daten");
            if (info > 0)
               Info_MAR(firstlevel + 1, gmp.MAR, info);
         }
      }

      static void Info_GMP(BinaryReaderWriter br, int info) {
         StdFile_GMP gmp = new StdFile_GMP();
         gmp.Read(br, true);
         Info_GMP(1, gmp, info);
      }

      #endregion


      //static void dem_test(BinaryReaderWriter br, string file) {
      //   StdFile_DEM dem = new StdFile_DEM();
      //   dem.Read(br);

      //   Info_ShowInfoItem(1, "Headerlänge", dem.Headerlength);
      //   Info_ShowInfoItem(1, "Höhen in Metern", !dem.HeigthInFeet);
      //   for (int i = 0; i < dem.Zoomleveltable.Records.Count; i++) {
      //      StdFile_DEM.ZoomlevelTable.Record record3 = dem.Zoomleveltable.Records[i];
      //      Info_ShowInfoItem(1, "Datenbereich", i);
      //      Info_ShowInfoItem(2, "Höhenbereich", record3.MinimumHeight.ToString() + " .. " + record3.MaximumHeight.ToString());
      //      Info_ShowInfoItem(2, "Kacheln", (record3.MaxTileIdxX + 1).ToString() + " x " + (record3.MaxTileIdxY + 1).ToString() + " = " + ((record3.MaxTileIdxX + 1) * (record3.MaxTileIdxY + 1)).ToString());
      //      Info_ShowInfoItem(2, "Kachelgröße", record3.PointsX.ToString() + " x " +
      //                                          record3.PixelsY.ToString() + " Pixel, " +
      //                                          StdFile_DEM.ZoomlevelTable.Record.Units2Degree(record3.PointsX * record3.PixelDistanceX).ToString() + "° / " +
      //                                          StdFile_DEM.ZoomlevelTable.Record.Units2Degree(record3.PixelsY * record3.PixelDistanceY).ToString() + "°");
      //      Info_ShowInfoItem(2, "Ecke links-oben", StdFile_DEM.ZoomlevelTable.Record.Units2Degree(record3.West).ToString() + "° / " +
      //                                              StdFile_DEM.ZoomlevelTable.Record.Units2Degree(record3.North).ToString() + "°");
      //      Info_ShowInfoItem(2, "Satzlänge", record3.Block1RecordSize.ToString() + " Byte");


      //      Info_ShowInfoItem(2, "Unknown_0x0A", record3.Unknown_0x0A);
      //      Info_ShowInfoItem(2, "Unknown_0x0E", record3.Unknown_0x0E);
      //      Info_ShowInfoItem(2, "Unknown_0x12", record3.Unknown_0x12);
      //   }
      //   for (int i = 0; i < dem.Tiles.Count; i++) {
      //      StdFile_DEM.ZoomlevelTable.Record record3 = dem.Zoomleveltable.Records[i];
      //      if ((record3.MaxTileIdxX + 1) * (record3.MaxTileIdxY + 1) != dem.Tiles[i].Records.Count) {
      //         Info_ShowInfoItem(1, "Datenbereich", i);
      //         Info_ShowInfoItem(1, "Tiles", dem.Tiles[i].Records.Count);
      //      }
      //   }

      //   for (int k = 0; k < dem.Tiles.Count; k++)
      //      for (int i = 0; i < dem.Tiles[k].Records.Count; i++) {
      //         StdFile_DEM.TileTable.Tile record1 = dem.Tiles[k].Records[i];
      //         Console.WriteLine(string.Format("Datenbereich {0}, Tile-Index {1}, BaseHeight 0x{2:X}, Diff2Max 0x{3:X}, DataLength {4}, Unknown 0x{5:X}",
      //            k,
      //            i,
      //            record1.BaseHeight,
      //            record1.Diff,
      //            record1.DataLength,
      //            record1.ExtCodingInfo
      //            ));

      //         string datafile = file + string.Format(" {0}_{1:D4}.bin",
      //                                          k,
      //                                          i
      //                                          );
      //         if (record1.DataLength > 0) {
      //            FileStream stream = null;
      //            BinaryReaderWriter bw = null;
      //            try {
      //               stream = File.Create(datafile);
      //               bw = new BinaryReaderWriter(stream);
      //               bw.Write(record1.Data);
      //            } finally {
      //               if (bw != null)
      //                  bw.Dispose(); // implizit auch stream
      //               else
      //                  stream?.Dispose();
      //            }
      //         }
      //      }





      //}


      #region InfoItems anzeigen

      static void Info_ShowInfoItem(uint indent, string name, string txt) {
         StringBuilder sb = new StringBuilder();
         while (indent-- > 0)
            sb.Append("  ");
         if (name.Length > 0) {
            sb.Append(name);
            sb.Append(":");
         }
         while (sb.Length < 23)
            sb.Append(" ");
         sb.Append(txt);
         Info_ShowInfoItem(sb.ToString());
      }

      static void Info_ShowInfoItem(uint indent, string txt) {
         StringBuilder sb = new StringBuilder();
         while (indent-- > 0)
            sb.Append("  ");
         sb.Append(txt);
         Info_ShowInfoItem(sb.ToString());
      }

      static void Info_ShowInfoItem(uint indent, string name, bool data) {
         Info_ShowInfoItem(indent, name, data.ToString());
      }

      static void Info_ShowInfoItem(uint indent, string name, DateTime dt) {
         Info_ShowInfoItem(indent, name, dt.ToShortDateString() + " " + dt.ToLongTimeString());
      }

      static void Info_ShowInfoItem(uint indent, string name, byte data, bool hex = false) {
         Info_ShowInfoItem(indent, name, data.ToString() + (hex ? " (0x" + data.ToString("X2") + ")" : ""));
      }

      static void Info_ShowInfoItem(uint indent, string name, byte[] data, bool hex = false) {
         StringBuilder sb = new StringBuilder();
         for (int i = 0; i < data.Length; i++) {
            if (i > 0)
               sb.Append(" ");
            sb.AppendFormat("{0}", data[i]);
         }
         if (hex) {
            sb.Append(" (");
            for (int i = 0; i < data.Length; i++) {
               if (i > 0)
                  sb.Append(" ");
               sb.AppendFormat("0x{0:X2}", data[i]);
            }
            sb.Append(")");
         }
         Info_ShowInfoItem(indent, name, sb.ToString());
      }

      static void Info_ShowInfoItem(uint indent, string name, short data, bool hex = false) {
         Info_ShowInfoItem(indent, name, data.ToString() + (hex ? " (0x" + data.ToString("X4") + ")" : ""));
      }

      static void Info_ShowInfoItem(uint indent, string name, ushort data, bool hex = false) {
         Info_ShowInfoItem(indent, name, data.ToString() + (hex ? " (0x" + data.ToString("X4") + ")" : ""));
      }

      static void Info_ShowInfoItem(uint indent, string name, int data, bool hex = false) {
         Info_ShowInfoItem(indent, name, data.ToString() + (hex ? " (0x" + data.ToString("X8") + ")" : ""));
      }

      static void Info_ShowInfoItem(uint indent, string name, uint data, bool hex = false) {
         Info_ShowInfoItem(indent, name, data.ToString() + (hex ? " (0x" + data.ToString("X8") + ")" : ""));
      }

      static void Info_ShowInfoItem(uint indent, string name, DataBlock data) {
         string txt = string.Format("Offset 0x{0:X}, Länge {1}", data.Offset, data.Length);
         Info_ShowInfoItem(indent, name, txt);
      }

      static void Info_ShowInfoItem(uint indent, string name, DataBlockWithRecordsize data) {
         string txt = string.Format("Offset 0x{0:X}, Länge {1}, Satzlänge {2}", data.Offset, data.Length, data.Recordsize);
         Info_ShowInfoItem(indent, name, txt);
      }

      static void Info_ShowInfoItem(string txt) {
         Console.WriteLine(txt);
      }

      #endregion
   }
}
