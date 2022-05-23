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

namespace GmTool {
   class AnalyzeTypes {

      public SortedList<int, int> PointType;
      public SortedList<int, int> LineType;
      public SortedList<int, int> AreaType;

      public SortedList<int, int> PointTypeSum;
      public SortedList<int, int> LineTypeSum;
      public SortedList<int, int> AreaTypeSum;

      public SortedList<int, SortedSet<string>> PointText4Type;
      public SortedList<int, SortedSet<string>> LineText4Type;
      public SortedList<int, SortedSet<string>> AreaText4Type;

      public SortedList<int, SortedSet<string>> PointText4TypeSum;
      public SortedList<int, SortedSet<string>> LineText4TypeSum;
      public SortedList<int, SortedSet<string>> AreaText4TypeSum;


      public AnalyzeTypes() {
         PointType = new SortedList<int, int>();
         LineType = new SortedList<int, int>();
         AreaType = new SortedList<int, int>();

         PointTypeSum = new SortedList<int, int>();
         LineTypeSum = new SortedList<int, int>();
         AreaTypeSum = new SortedList<int, int>();

         PointText4Type = new SortedList<int, SortedSet<string>>();
         LineText4Type = new SortedList<int, SortedSet<string>>();
         AreaText4Type = new SortedList<int, SortedSet<string>>();

         PointText4TypeSum = new SortedList<int, SortedSet<string>>();
         LineText4TypeSum = new SortedList<int, SortedSet<string>>();
         AreaText4TypeSum = new SortedList<int, SortedSet<string>>();
      }

      public void Clear() {
         PointType.Clear();
         LineType.Clear();
         AreaType.Clear();

         PointText4Type.Clear();
         LineText4Type.Clear();
         AreaText4Type.Clear();
      }

      /// <summary>
      /// analysiert die Typen dieser Datei
      /// </summary>
      /// <param name="file"></param>
      public void AnalyzingTypes(string file) {
         FileInfo fi = new FileInfo(file);
         if (!fi.Exists) {
            Console.WriteLine("Die Datei '" + file + "' existiert nicht.");
            return;
         }

         using (BinaryReaderWriter br = new BinaryReaderWriter(file, true)) {
            string ext = fi.Extension.ToUpper();
            BinaryReaderWriter brlbl = null;
            BinaryReaderWriter brtre = null;

            if (ext == ".IMG") {

               SimpleFilesystem sf = new SimpleFilesystem();
               sf.Read(br);

               for (int i = 0; i < sf.FileCount; i++) {
                  string subfile = sf.Filename(i);
                  ext = subfile.Substring(subfile.Length - 4).ToUpper();

                  if (ext == ".RGN") {
                     Console.WriteLine("Sub-Datei '" + subfile + "'");
                     uint flen = sf.Filesize(i);
                     Console.WriteLine(string.Format("{0} Bytes ({1:F1} kB, {2:F1} MB)",
                                                     flen,
                                                     flen / 1024.0,
                                                     flen / (1024.0 * 1024)));
                     using (BinaryReaderWriter brf = sf.GetBinaryReaderWriter4File(subfile)) {
                        Console.Error.WriteLine("analysiere " + Path.GetFileNameWithoutExtension(subfile) + " ...");
                        string basefilename = subfile.Substring(0, 8);
                        brtre = brlbl = null;
                        brtre = sf.GetBinaryReaderWriter4File(basefilename + ".TRE");
                        brlbl = sf.GetBinaryReaderWriter4File(basefilename + ".LBL");
                        AnalyzeFile(brf, brtre, brlbl, false);
                        if (brlbl != null)
                           brlbl.Dispose();
                        if (brtre != null)
                           brtre.Dispose();

                        brf.Dispose();
                     }
                  }
               }

            } else if (ext == ".RGN") {

               Console.Error.WriteLine("analysiere " + Path.GetFileNameWithoutExtension(file) + " ...");
               brtre = brlbl = null;
               string basefilename = file.Substring(0, 8);
               if (File.Exists(basefilename + ".TRE"))
                  brtre = new BinaryReaderWriter(basefilename + ".TRE", true);
               if (File.Exists(basefilename + ".LBL"))
                  brlbl = new BinaryReaderWriter(basefilename + ".LBL", true);
               AnalyzeFile(br, brtre, brlbl, false);
               if (brlbl != null)
                  brlbl.Dispose();
               if (brtre != null)
                  brtre.Dispose();

            }

            br.Dispose();
         }
      }

      /// <summary>
      /// zeigt das Gesamtergebnis der Analyse an
      /// </summary>
      /// <param name="full">bei true erweiterte Anzeige</param>
      public void ShowAnalyzingTypesResult(bool full) {
         Console.Error.WriteLine("Ausgabe ...");
         Console.WriteLine("gesamt:");
         Console.WriteLine("Punkttypen");
         foreach (var item in PointTypeSum) {
            Console.WriteLine(string.Format(" {0,5:X} {1}x", item.Key, item.Value));
            if (full)
               foreach (string txt in PointText4TypeSum[item.Key])
                  if (!string.IsNullOrEmpty(txt))
                     Console.WriteLine(string.Format("     {0}", txt));
         }
         Console.WriteLine("Linientypen");
         foreach (var item in LineTypeSum) {
            Console.WriteLine(string.Format(" {0,5:X} {1}x", item.Key, item.Value));
            if (full)
               foreach (string txt in LineText4TypeSum[item.Key])
                  if (!string.IsNullOrEmpty(txt))
                     Console.WriteLine(string.Format("     {0}", txt));
         }
         Console.WriteLine("Flächentypen");
         foreach (var item in AreaTypeSum) {
            Console.WriteLine(string.Format(" {0,5:X} {1}x", item.Key, item.Value));
            if (full)
               foreach (string txt in AreaText4TypeSum[item.Key])
                  if (!string.IsNullOrEmpty(txt))
                     Console.WriteLine(string.Format("     {0}", txt));
         }
      }

      void AnalyzeFile(BinaryReaderWriter brrgn, BinaryReaderWriter brtre, BinaryReaderWriter brlbl, bool show = false) {
         Clear();

         StdFile_TRE tre = new StdFile_TRE();
         if (brtre != null)
            tre.Read(brtre);

         StdFile_RGN rgn = new StdFile_RGN(tre);
         if (brrgn != null)
            rgn.Read(brrgn);

         StdFile_LBL lbl = new StdFile_LBL();
         if (brlbl != null)
            lbl.Read(brlbl);

         int typ;
         for (int i = 0; i < rgn.SubdivList.Count && i < tre.SubdivInfoList.Count; i++) {
            StdFile_RGN.SubdivData sd = rgn.SubdivList[i];

            foreach (var item in sd.AreaList) {
               typ = item.Type << 8 | item.Subtype;
               string txt = null;
               if (item.LabelOffsetInLBL != 0 && lbl.TextList.Count > 0)
                  if (!item.LabelInNET)            // das dürfte immer so sein
                     txt = lbl.GetText(item.LabelOffsetInLBL, false);
               RegisterArea(typ, txt);
            }

            foreach (var item in sd.ExtAreaList) {
               typ = ((0x100 | item.Type) << 8) | item.Subtype;
               string txt = null;
               if (item.HasLabel && lbl.TextList.Count > 0)
                  txt = lbl.GetText(item.LabelOffsetInLBL, false);
               RegisterArea(typ, txt);
            }

            foreach (var item in sd.LineList) {
               typ = item.Type << 8 | item.Subtype;
               string txt = null;
               if (item.LabelOffsetInLBL != 0 && lbl.TextList.Count > 0)
                  if (!item.LabelInNET)
                     txt = lbl.GetText(item.LabelOffsetInLBL, false);
               //   else
               //      p.NetData = new DetailMap.RoadDataExt(net.Roaddata[net.Idx4Offset[item.LabelOffset]], lbl);
               RegisterLine(typ, txt);
            }

            foreach (var item in sd.ExtLineList) {
               typ = ((0x100 | item.Type) << 8) | item.Subtype;
               string txt = null;
               if (item.HasLabel && lbl.TextList.Count > 0)
                  txt = lbl.GetText(item.LabelOffsetInLBL, false);
               RegisterLine(typ, txt);
            }

            foreach (var item in sd.PointList2) {      // vor den "normalen" Punkten einlesen, damit der ev. Index-Verweise stimmen (z.B. für Exits)
               typ = item.Type << 8 | item.Subtype;
               string txt = null;
               if (item.LabelOffsetInLBL != 0 && lbl.TextList.Count > 0)
                  if (!item.IsPoiOffset)
                     txt = lbl.GetText(item.LabelOffsetInLBL, false);
               //   else {
               //      int idx = lbl.POIPropertiesListOffsets[item.LabelOffset];
               //      DetailMap.PoiDataExt pd = new DetailMap.PoiDataExt(lbl.POIPropertiesList[idx], lbl);
               //      p.LblData = pd;
               //      p.Label = p.LblData.Text;
               //   }
               RegisterPoint(typ, txt);
            }

            foreach (var item in sd.PointList1) {
               typ = item.Type << 8 | item.Subtype;
               string txt = null;
               if (item.LabelOffsetInLBL != 0 && lbl.TextList.Count > 0)
                  if (!item.IsPoiOffset)
                     txt = lbl.GetText(item.LabelOffsetInLBL, false);
               //   else {
               //      int idx = lbl.POIPropertiesListOffsets[item.LabelOffset];
               //      DetailMap.PoiDataExt pd = new DetailMap.PoiDataExt(lbl.POIPropertiesList[idx], lbl);
               //      p.LblData = pd;
               //      p.Label = p.LblData.Text;
               //   }
               RegisterPoint(typ, txt);
            }

            foreach (var item in sd.ExtPointList) {
               typ = ((0x100 | item.Type) << 8) | item.Subtype;
               string txt = null;
               if (item.HasLabel && lbl.TextList.Count > 0)
                  txt = lbl.GetText(item.LabelOffsetInLBL, false);
               RegisterPoint(typ, txt);
            }
         }

         if (show) {
            Console.WriteLine("Punkttypen");
            foreach (var item in PointType)
               Console.WriteLine(string.Format(" {0,5:X} {1}x", item.Key, item.Value));
            Console.WriteLine("Linientypen");
            foreach (var item in LineType)
               Console.WriteLine(string.Format(" {0,5:X} {1}x", item.Key, item.Value));
            Console.WriteLine("Flächentypen");
            foreach (var item in AreaType)
               Console.WriteLine(string.Format(" {0,5:X} {1}x", item.Key, item.Value));
         }

         BuildSum();
      }

      void RegisterPoint(int type, string text) {
         Register(type, text, PointType, PointText4Type);
      }

      void RegisterLine(int type, string text) {
         Register(type, text, LineType, LineText4Type);
      }

      void RegisterArea(int type, string text) {
         Register(type, text, AreaType, AreaText4Type);
      }

      void Register(int type, string text, SortedList<int, int> typelist, SortedList<int, SortedSet<string>> textlist) {
         if (typelist.ContainsKey(type))
            typelist[type]++;
         else
            typelist.Add(type, 1);
         if (textlist.ContainsKey(type)) {
            if (!textlist[type].Contains(text))
               textlist[type].Add(text);
         } else
            textlist.Add(type, new SortedSet<string>(new string[] { text }));
      }

      void BuildSum() {
         foreach (var item in PointType)
            if (!PointTypeSum.ContainsKey(item.Key))
               PointTypeSum.Add(item.Key, item.Value);
            else
               PointTypeSum[item.Key] += item.Value;

         foreach (var item in LineType)
            if (!LineTypeSum.ContainsKey(item.Key))
               LineTypeSum.Add(item.Key, item.Value);
            else
               LineTypeSum[item.Key] += item.Value;

         foreach (var item in AreaType)
            if (!AreaTypeSum.ContainsKey(item.Key))
               AreaTypeSum.Add(item.Key, item.Value);
            else
               AreaTypeSum[item.Key] += item.Value;

         foreach (var item in PointText4Type)
            if (!PointText4TypeSum.ContainsKey(item.Key))
               PointText4TypeSum.Add(item.Key, new SortedSet<string>(item.Value));
            else
               foreach (string txt in item.Value)
                  if (!PointText4TypeSum[item.Key].Contains(txt))
                     PointText4TypeSum[item.Key].Add(txt);

         foreach (var item in LineText4Type)
            if (!LineText4TypeSum.ContainsKey(item.Key))
               LineText4TypeSum.Add(item.Key, new SortedSet<string>(item.Value));
            else
               foreach (string txt in item.Value)
                  if (!LineText4TypeSum[item.Key].Contains(txt))
                     LineText4TypeSum[item.Key].Add(txt);

         foreach (var item in AreaText4Type)
            if (!AreaText4TypeSum.ContainsKey(item.Key))
               AreaText4TypeSum.Add(item.Key, new SortedSet<string>(item.Value));
            else
               foreach (string txt in item.Value)
                  if (!AreaText4TypeSum[item.Key].Contains(txt))
                     AreaText4TypeSum[item.Key].Add(txt);

      }

   }
}
