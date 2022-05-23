using System;
using System.Collections.Generic;
using System.Globalization;

namespace GmTool {

   /// <summary>
   /// Optionen und Argumente werden zweckmäßigerweise in eine (programmabhängige) Klasse gekapselt.
   /// Erzeugen des Objektes und Evaluate() sollten in einem try-catch-Block erfolgen.
   /// </summary>
   public class Options {

      // alle Optionen sind 'read-only'

      /// <summary>
      /// Eingabedatei/en oder -pfad
      /// </summary>
      public string[] Input { get; private set; }

      /// <summary>
      /// Eingabedateien werden auch in Unterverzeichnissen gesucht
      /// </summary>
      public bool InputWithSubdirs { get; private set; }

      /// <summary>
      /// Ausgabedatei oder -pfad
      /// </summary>
      public string Output { get; private set; }

      /// <summary>
      /// Ausgabeziel ev. überschreiben
      /// </summary>
      public bool OutputOverwrite { get; private set; }

      /// <summary>
      /// Art der Aktion
      /// </summary>
      public enum ToDoType {
         Nothing,
         /// <summary>
         /// Info über die Inputdatei/en
         /// </summary>
         Info,
         /// <summary>
         /// ausführliche Info über die Inputdatei/en
         /// </summary>
         LongInfo,
         /// <summary>
         /// besonders ausführliche Info über die Inputdatei/en
         /// </summary>
         ExtLongInfo,
         VeryLongInfo,

         /// <summary>
         /// IMG-Datei in einzelne Dateien aufteilen
         /// </summary>
         Split,
         /// <summary>
         /// IMG-Datei rekursiv in einzelne Dateien aufteilen
         /// </summary>
         SplitRecursive,
         /// <summary>
         /// IMG-Datei in einzelne Dateien aufteilen und zusammengehörende TRE-, LBL-, RGN-, NET-, NOD-, DEM- und MAR-Dateien wieder zu IMG-Dateien verbinden
         /// </summary>
         SplitJoin,
         /// <summary>
         /// IMG-Datei rekursiv in einzelne Dateien aufteilen und zusammengehörende TRE-, LBL-, RGN-, NET-, NOD-, DEM- und MAR-Dateien wieder zu IMG-Dateien verbinden
         /// </summary>
         SplitRecursiveJoin,
         /// <summary>
         /// alle Dateien zu einer Geräte- oder Kachel-IMG (bzw. GMP) zusammenfügen
         /// </summary>
         Join,
         /// <summary>
         /// alle Dateien als Device-IMG zusammenführen
         /// </summary>
         JoinDevice,
         /// <summary>
         /// alle Dateien als Kachel zusammenführen
         /// </summary>
         JoinTile,

         /// <summary>
         /// zusätzliche Dateien für Mapsource erzeugen
         /// </summary>
         CreateFiles4Mapsource,

         /// <summary>
         /// Refresh (i.W. die Dateiliste) einer TDB
         /// </summary>
         RefreshTDB,

         /// <summary>
         /// die in den geografischen Daten verwendeten Typen ermitteln
         /// </summary>
         AnalyzingTypes,
         /// <summary>
         /// die in den geografischen Daten verwendeten Typen und alle Bezeichnungen ermitteln
         /// </summary>
         AnalyzingTypesLong,

         /// <summary>
         /// setzte eine neue TYP-Datei
         /// </summary>
         SetNewTypfile,

         /// <summary>
         /// Ändern von Dateieigenschaften
         /// </summary>
         Change,
      }

      /// <summary>
      /// Art des Aufteilens
      /// </summary>
      public ToDoType ToDo { get; private set; }

      /// <summary>
      /// gibt die Overview-IMG-Datei bzw. das Verzeichnis mit der Overview-TRE-Datei an
      /// </summary>
      public string OverviewImgOrPath { get; private set; }


      public class Property {
         /// <summary>
         /// Soll die Eigenschaft neu gesetzt werden?
         /// </summary>
         public bool IsSet { get; private set; }
         /// <summary>
         /// Eigenschaft
         /// </summary>
         public object Value { get; private set; }
         /// <summary>
         /// liefert die Eigenschaft als String, wenn sie ein String und gesetzt ist (sonst null)
         /// </summary>
         public string ValueAsString {
            get {
               if (IsSet && Value.GetType() == typeof(string))
                  return Value as string;
               return null;
            }
         }
         /// <summary>
         /// liefert die Eigenschaft als Int, wenn sie ein Int und gesetzt ist (Int.MinValue)
         /// </summary>
         public int ValueAsInt {
            get {
               if (IsSet && Value.GetType() == typeof(int))
                  return (int)Value;
               return int.MaxValue;
            }
         }

         public uint ValueAsUInt {
            get {
               if (IsSet && Value.GetType() == typeof(uint))
                  return (uint)Value;
               return uint.MaxValue;
            }
         }

         public bool ValueAsBool {
            get {
               if (IsSet && Value.GetType() == typeof(bool))
                  return (bool)Value;
               return false;
            }
         }

         public Property(object value = null, bool set = false) {
            Set(value, set);
         }

         public void Set(object value, bool set = true) {
            Value = value;
            IsSet = set;
         }

         public override string ToString() {
            return string.Format("IsSet={0}, [{1}]", IsSet, Value);
         }

      }

      public Property PID { get; private set; }

      public Property FID { get; private set; }

      public Property Codepage { get; private set; }

      public List<Property> TDBCopyrightCodes { get; private set; }

      public List<Property> TDBCopyrightWhereCodes { get; private set; }

      public List<Property> TDBCopyrightText { get; private set; }

      public Property Description { get; private set; }

      public Property Priority { get; private set; }

      public Property Transparent { get; private set; }

      public Property MapFamilyName { get; private set; }

      public Property MapSeriesName { get; private set; }

      public Property Version { get; private set; }

      public Property Routable { get; private set; }

      public Property HighestRoutable { get; private set; }

      public Property HasDEM { get; private set; }

      public Property HasProfile { get; private set; }

      public Property MaxCoordBits4Overview { get; private set; }

      // zusätzliche Optionen für die erzeugung von MS-Dateien

      //public Property MapsourcePID { get; private set; }
      //public Property MapsourceFID { get; private set; }
      //public Property MapsourceCodepage { get; private set; }
      public Property MapsourceMinDimension { get; private set; }
      //public Property MapsourceOverviewNo { get; private set; }
      public Property MapsourceOverviewfile { get; private set; }
      public Property MapsourceTYPfile { get; private set; }
      public Property MapsourceMDXfile { get; private set; }
      public Property MapsourceMDRfile { get; private set; }
      public Property MapsourceTDBfile { get; private set; }
      public SortedSet<int> MapsourceOVPointtypes { get; private set; }
      public SortedSet<int> MapsourceOVLinetypes { get; private set; }
      public SortedSet<int> MapsourceOVAreatypes { get; private set; }
      public Property MapsourceNoOverviewfile { get; private set; }
      public Property MapsourceNoTYPfile { get; private set; }
      public Property MapsourceNoMDXfile { get; private set; }
      public Property MapsourceNoMDRfile { get; private set; }
      public Property MapsourceNoTDBfile { get; private set; }
      public Property MapsourceNoInstfiles { get; private set; }

      public Property NewTypfile { get; private set; }

      FSoftUtils.CmdlineOptions cmd;

      enum MyOptions {
         Input,
         InputListfile,
         InputWithSubdirs,
         Output,
         OutputOverwrite,

         Info,
         Split,
         Join,
         CreateFiles4Mapsource,
         AnalyzingTypes,

         SetPID,
         SetFID,
         SetTDBCopyright,
         SetDescription,
         SetMapFamilyName,
         SetMapSeriesName,
         SetCodepage,
         SetPriority,
         SetTransparent,
         SetVersion,
         SetRoutable,
         SetHighestRoutable,
         SetHasDEM,
         SetHasProfile,
         SetMaxCoordBits4Overview,

         RefreshTDB,

         NewTypfile,

         Help,
      }

      public Options() {
         Init();
         cmd = new FSoftUtils.CmdlineOptions();
         // Definition der Optionen
         //cmd.DefineOption((int)MyOptions.MyBoolA, "boola", "a", "true, wenn verwendet", FsoftUtils.CmdlineOptions.OptionArgumentType.Nothing);
         //cmd.DefineOption((int)MyOptions.MyBoolB, "boolb", "b", "true,\nwenn verwendet", FsoftUtils.CmdlineOptions.OptionArgumentType.Nothing);
         //cmd.DefineOption((int)MyOptions.MyBoolC, "boolc", "c", "true,\nwenn\nverwendet", FsoftUtils.CmdlineOptions.OptionArgumentType.Nothing);
         //cmd.DefineOption((int)MyOptions.MyBoolexpl, "boolexpl", "B", "explizit true oder false", FsoftUtils.CmdlineOptions.OptionArgumentType.Boolean);
         //cmd.DefineOption((int)MyOptions.MyInt, "int", "", "integer", FsoftUtils.CmdlineOptions.OptionArgumentType.Integer);
         //cmd.DefineOption((int)MyOptions.MyPInt, "pint", "", "positiv integer", FsoftUtils.CmdlineOptions.OptionArgumentType.PositivInteger);
         //cmd.DefineOption((int)MyOptions.MyUInt, "uint", "", "unsigned integer", FsoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         //cmd.DefineOption((int)MyOptions.MyDouble, "double", "", "double", FsoftUtils.CmdlineOptions.OptionArgumentType.Double);
         //cmd.DefineOption((int)MyOptions.MyString, "string", "", "string", FsoftUtils.CmdlineOptions.OptionArgumentType.String);
         //cmd.DefineOption((int)MyOptions.MyMString, "mstring", "m", "string\nmehrfach verwendbar", FsoftUtils.CmdlineOptions.OptionArgumentType.String, int.MaxValue);


         cmd.DefineOption((int)MyOptions.Input, "input", "i", "Eingabedatei (mehrfach verwendbar, auch * und ?)", FSoftUtils.CmdlineOptions.OptionArgumentType.String, int.MaxValue);
         cmd.DefineOption((int)MyOptions.InputListfile, "inputlist", "", "Textdatei mit den Eingabedateien", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.InputWithSubdirs, "withsubdirs", "", "Eingabedateien auch in Unterverzeichnissen suchen (Standard true)", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNothing);
         cmd.DefineOption((int)MyOptions.Output, "output", "o", "Ausgabeziel", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.OutputOverwrite, "overwrite", "O", "Ausgabeziel bei Bedarf überschreiben (ohne Argument 'true', Standard 'false')", FSoftUtils.CmdlineOptions.OptionArgumentType.BooleanOrNothing);

         cmd.DefineOption((int)MyOptions.Info, "info", "I", "zeigt Infos zu den Eingabedaten an (ohne Argument 0, Standard 0)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedIntegerOrNothing);
         cmd.DefineOption((int)MyOptions.Split, "split", "s", "teilt eine IMG-Datei in die einzelnen Dateien auf (Standard ohne 'r' und 'j')\n" +
                                                              "   r   teilt rekursiv zusätzlich auch in der IMG-Datei enthaltene GMP/IMG-Dateien auf\n" +
                                                              "   j   verbindet die beim Aufteilen jeweils zusammengehörenden\n" +
                                                              "       TRE-, LBL-, RGN-, NET-, NOD-, DEM- und MAR-Dateien wieder zu einer IMG-Datei", FSoftUtils.CmdlineOptions.OptionArgumentType.StringOrNothing);
         cmd.DefineOption((int)MyOptions.Join, "join", "j", "verbindet die Dateien zu einer Geräte- oder Kachel-IMG-Datei (bzw. GMP)\n" +
                                                            "   device  erzeugt eine Device-IMG\n" +
                                                            "   tile    erzeugt eine Kachel-Datei", FSoftUtils.CmdlineOptions.OptionArgumentType.StringOrNothing);
         cmd.DefineOption((int)MyOptions.CreateFiles4Mapsource, "mapsource", "m", "erzeugt Dateien für Mapsource/Basecamp (mehrfach verwendbar)\n" +
                                                                                  "weitere Sub-Optionen (ev. mit ';' getrennt):\n" +
                                                                                  "   pid:zahl      Product-ID setzen\n" +
                                                                                  "   fid:zahl      Family-ID setzen\n" +
                                                                                  "   cp:zahl       Codepage setzen\n" +
                                                                                  "   points:typelist Liste der gewünschten Punkttypen für die Overviewkarte (Standard 0x400, 0x600)\n" +
                                                                                  "   lines:typelist  Liste der gewünschten Linientypen für die Overviewkarte (Standard 0x100, 0x200, 0x300, 0x400, 0x1F00)\n" +
                                                                                  "   areas:typelist  Liste der gewünschten Flächentypen für die Overviewkarte (Standard 0x3200, 0x5000)\n" +
                                                                                  "   mindim:zahl   Mindestgröße der Umgrenzung der Linien und Flächen für die Overviewkarte (Standard 2)\n" +
                                                                                  "   ov:name       Dateiname der Overviewdatei (Standard: gmtool.img)\n" +
                                                                                  "   typ:name      Dateiname der TYP-Datei (Standard: gmtool.typ)\n" +
                                                                                  "   tdb:name      Dateiname der TDB-Datei (Standard: gmtool.tdb)\n" +
                                                                                  "   mdx:name      Dateiname der MDX-Datei (Standard: gmtool.mdx)\n" +
                                                                                  "   mdr:name      Dateiname der MDR-Datei (Standard: gmtool_mdr.img)\n" +
                                                                                  "   noov          keine Overviewdatei erzeugen\n" +
                                                                                  "   notyp         keine TYP-Datei erzeugen\n" +
                                                                                  "   notdb         keine TDB-Datei erzeugen\n" +
                                                                                  "   nomdx         keine MDX-Datei erzeugen\n" +
                                                                                  "   nomdr         keine MDR-Datei erzeugen\n" +
                                                                                  "   noinst        keine Installations-Dateien erzeugen",
                                                                                  FSoftUtils.CmdlineOptions.OptionArgumentType.StringOrNothing, int.MaxValue);
         cmd.DefineOption((int)MyOptions.AnalyzingTypes, "analyzingtypes", "a", "Analyse der verwendeten Objekttypen in den geografischen Daten (RGN-Datei, ...)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);

         cmd.DefineOption((int)MyOptions.SetPID, "pid", "p", "setzt die PID in TYP-, MDX-, MPS- und TDB-Dateien (i.A. 1)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetFID, "fid", "f", "setzt die FID in TYP-, MDX-, MPS- und TDB-Dateien", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetTDBCopyright, "copyright", "c", "setzt ein Copyright-Segment in TDB-Dateien: [S|C|*][I|P|E|*][N|D|*][neuer Text] (mehrfach verwendbar)\n" +
                                                                            "   S   SourceInformation\n" +
                                                                            "   C   CopyrightInformation\n" +
                                                                            "   I   ProductInformation\n" +
                                                                            "   P   Printing\n" +
                                                                            "   E   ProductInformation & Printing (ever)\n" +
                                                                            "   N   neuer Text\n" +
                                                                            "   D   Copyright-Segment löschen\n" +
                                                                            "   *   keine Änderung", FSoftUtils.CmdlineOptions.OptionArgumentType.String, int.MaxValue);
         cmd.DefineOption((int)MyOptions.SetDescription, "description", "", "setzt den Beschreibungstext in IMG- und TDB-Dateien", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.SetCodepage, "codepage", "", "setzt die Codepage in TYP- und TDB-Dateien (i.A. 1252)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetPriority, "priority", "", "setzt die Anzeigepriorität in TRE-Dateien (i.A. Layer 0..25)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetTransparent, "transparent", "t", "setzt die Transparenz (0x4B-Flächen in RGN-Dateien)", FSoftUtils.CmdlineOptions.OptionArgumentType.Boolean);
         cmd.DefineOption((int)MyOptions.SetMapFamilyName, "mapfamilyname", "", "setzt den Kartenname in TDB-Dateien", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.SetMapSeriesName, "mapseriesname", "", "setzt den Name der Kartenserie in TDB-Dateien", FSoftUtils.CmdlineOptions.OptionArgumentType.String);
         cmd.DefineOption((int)MyOptions.SetVersion, "version", "", "setzt die Versionsnummer in TDB-Dateien (i.A. 100 für 1.00)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetRoutable, "routable", "", "setzt die Routable-Eigenschaft in TDB-Dateien (i.A. 0 oder 1)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetHasDEM, "hasdem", "", "setzt die HasDEM-Eigenschaft in TDB-Dateien (i.A. 0 oder 1)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetHasProfile, "hasprofile", "", "setzt die HasProfile-Eigenschaft in TDB-Dateien (i.A. 0 oder 1)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetHighestRoutable, "highestroutable", "", "setzt die HighestRoutable-Eigenschaft in TDB-Dateien (z.B. 0x18)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);
         cmd.DefineOption((int)MyOptions.SetMaxCoordBits4Overview, "maxbits4overview", "", "setzt die max. Bitanzahl der Koordinaten für das Anzeigen der Overviewkarte in TDB-Dateien (z.B. 18)", FSoftUtils.CmdlineOptions.OptionArgumentType.UnsignedInteger);

         cmd.DefineOption((int)MyOptions.RefreshTDB, "refreshtdb", "", "liest (i.W.) die Dateiliste einer TDB neu ein", FSoftUtils.CmdlineOptions.OptionArgumentType.Nothing);

         cmd.DefineOption((int)MyOptions.NewTypfile, "newtypfile", "", "ersetzt die TYP-Datei einer IMG-Datei durch eine neue", FSoftUtils.CmdlineOptions.OptionArgumentType.String);

         cmd.DefineOption((int)MyOptions.Help, "help", "?", "diese Hilfe", FSoftUtils.CmdlineOptions.OptionArgumentType.Nothing);
      }

      /// <summary>
      /// Standardwerte setzen
      /// </summary>
      void Init() {
         Input = new string[0];
         InputWithSubdirs = false;
         Output = "";
         OutputOverwrite = false;

         ToDo = ToDoType.Nothing;

         OverviewImgOrPath = "";

         PID = new Property();
         FID = new Property();
         Codepage = new Property();
         TDBCopyrightText = new List<Property>();
         TDBCopyrightCodes = new List<Property>();
         TDBCopyrightWhereCodes = new List<Property>();
         Description = new Property();
         Priority = new Property();
         Transparent = new Property();
         MapFamilyName = new Property();
         MapSeriesName = new Property();
         Version = new Property();
         Routable = new Property();
         HighestRoutable = new Property();
         HasDEM = new Property();
         HasProfile = new Property();
         MaxCoordBits4Overview = new Property();

         //MapsourcePID = new Property();
         //MapsourceFID = new Property();
         //MapsourceCodepage = new Property();
         //MapsourceOverviewNo = new Property();

         MapsourceMinDimension = new Property();
         MapsourceOverviewfile = new Property();
         MapsourceTYPfile = new Property();
         MapsourceMDXfile = new Property();
         MapsourceMDRfile = new Property();
         MapsourceTDBfile = new Property();

         MapsourceNoOverviewfile = new Property();
         MapsourceNoTYPfile = new Property();
         MapsourceNoMDXfile = new Property();
         MapsourceNoMDRfile = new Property();
         MapsourceNoTDBfile = new Property();
         MapsourceNoInstfiles = new Property();

         NewTypfile = new Property();
}

      /// <summary>
      /// Auswertung der Optionen
      /// </summary>
      /// <param name="args"></param>
      public void Evaluate(string[] args) {
         if (args == null) return;
         List<string> InputArray_Tmp = new List<string>();

         try {
            cmd.Parse(args);

            foreach (MyOptions opt in Enum.GetValues(typeof(MyOptions))) {    // jede denkbare Option testen
               int optcount = cmd.OptionAssignment((int)opt);                 // Wie oft wurde diese Option verwendet?
               string arg;
               if (optcount > 0)
                  switch (opt) {
                     case MyOptions.Input:
                        for (int i = 0; i < optcount; i++)
                           InputArray_Tmp.Add(cmd.StringValue((int)opt, i).Trim());
                        break;

                     case MyOptions.InputWithSubdirs:
                        if (cmd.ArgIsUsed((int)opt))
                           InputWithSubdirs = cmd.BooleanValue((int)opt);
                        else
                           InputWithSubdirs = true;
                        break;

                     case MyOptions.InputListfile:
                        InputArray_Tmp.AddRange(System.IO.File.ReadAllLines(cmd.StringValue((int)opt)));
                        for (int i = InputArray_Tmp.Count - 1; i >= 0; i--) {
                           InputArray_Tmp[i] = InputArray_Tmp[i].Trim();
                           if (InputArray_Tmp[i].Length == 0)
                              InputArray_Tmp.RemoveAt(i);
                        }
                        break;

                     case MyOptions.Output:
                        Output = cmd.StringValue((int)opt).Trim();
                        break;

                     case MyOptions.OutputOverwrite:
                        if (cmd.ArgIsUsed((int)opt))
                           OutputOverwrite = cmd.BooleanValue((int)opt);
                        else
                           OutputOverwrite = true;
                        break;

                     case MyOptions.Info:
                        if (cmd.ArgIsUsed((int)opt)) {
                           switch (cmd.UnsignedIntegerValue((int)opt)) {
                              case 0: ToDo = ToDoType.Info; break;
                              case 1: ToDo = ToDoType.LongInfo; break;
                              case 2: ToDo = ToDoType.ExtLongInfo; break;
                              default: ToDo = ToDoType.VeryLongInfo; break;
                           }
                        } else
                           ToDo = ToDoType.Info;
                        break;

                     case MyOptions.Split:
                        if (cmd.ArgIsUsed((int)opt)) {
                           arg = cmd.StringValue((int)opt);
                           if (arg == "r")
                              ToDo = ToDoType.SplitRecursive;
                           else if (arg == "j")
                              ToDo = ToDoType.SplitJoin;
                           else if (arg == "rj" || arg == "jr")
                              ToDo = ToDoType.SplitRecursiveJoin;
                        } else
                           ToDo = ToDoType.Split;
                        break;

                     case MyOptions.CreateFiles4Mapsource:
                        for (int i = 0; i < optcount; i++)
                           if (cmd.ArgIsUsed((int)opt, i)) {
                              arg = cmd.StringValue((int)opt, i);
                              if (!string.IsNullOrEmpty(arg)) {

                                 if (arg.StartsWith("pid:")) {
                                    PID.Set(InterpretUInt(arg));
                                 } else if (arg.StartsWith("fid:")) {
                                    FID.Set(InterpretUInt(arg));
                                 } else if (arg.StartsWith("cp:")) {
                                    Codepage.Set(InterpretUInt(arg));
                                    //} else if (arg.StartsWith("ovno:")) {
                                    //   MapsourceOverviewNo.Set(InterpretUInt(arg));

                                 } else if (arg.StartsWith("ov:")) {
                                    MapsourceOverviewfile.Set(arg.Substring(3));
                                 } else if (arg.StartsWith("typ:")) {
                                    MapsourceTYPfile.Set(arg.Substring(4));
                                 } else if (arg.StartsWith("tdb:")) {
                                    MapsourceTDBfile.Set(arg.Substring(4));
                                 } else if (arg.StartsWith("mdx:")) {
                                    MapsourceMDXfile.Set(arg.Substring(4));
                                 } else if (arg.StartsWith("mdr:")) {
                                    MapsourceMDRfile.Set(arg.Substring(4));
                                 } else if (arg.StartsWith("tdb:")) {
                                    MapsourceTDBfile.Set(arg.Substring(4));

                                 } else if (arg.StartsWith("mindim:")) {
                                    MapsourceMinDimension.Set(InterpretUInt(arg));
                                 } else if (arg.StartsWith("points:")) {
                                    InterpretTypes(arg, MapsourceOVPointtypes);
                                 } else if (arg.StartsWith("lines:")) {
                                    InterpretTypes(arg, MapsourceOVLinetypes);
                                 } else if (arg.StartsWith("areas:")) {
                                    InterpretTypes(arg, MapsourceOVAreatypes);

                                 } else if (arg == "noov") {
                                    MapsourceNoOverviewfile.Set(true);
                                 } else if (arg == "notyp") {
                                    MapsourceNoTYPfile.Set(true);
                                 } else if (arg == "nomdx") {
                                    MapsourceNoMDXfile.Set(true);
                                 } else if (arg == "nomdr") {
                                    MapsourceNoMDRfile.Set(true);
                                 } else if (arg == "notdb") {
                                    MapsourceNoTDBfile.Set(true);
                                 } else if (arg == "noinst") {
                                    MapsourceNoInstfiles.Set(true);

                                 } else
                                    throw new Exception("unbekanntes Argument: " + arg);

                              }
                           }
                        ToDo = ToDoType.CreateFiles4Mapsource;
                        break;

                     case MyOptions.Join:
                        if (cmd.ArgIsUsed((int)opt)) {
                           arg = cmd.StringValue((int)opt);
                           if (arg == "device")
                              ToDo = ToDoType.JoinDevice;
                           else if (arg == "tile")
                              ToDo = ToDoType.JoinTile;
                        } else
                           ToDo = ToDoType.Join;
                        break;

                     case MyOptions.AnalyzingTypes:
                        switch (cmd.UnsignedIntegerValue((int)opt)) {
                           case 1: ToDo = ToDoType.AnalyzingTypesLong; break;
                           default: ToDo = ToDoType.AnalyzingTypes; break;
                        }
                        break;


                     case MyOptions.SetPID:
                        PID.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetFID:
                        FID.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetCodepage:
                        Codepage.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetTDBCopyright:
                        for (int j = 0; j < optcount; j++) {
                           arg = cmd.StringValue((int)opt, j);
                           if (arg.Length < 3)
                              throw new Exception("Falscher Aufbau der Copyright-Option '" + arg + "'");
                           else {
                              switch (arg[0]) {
                                 case 'S':
                                    TDBCopyrightCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.CopyrightCodes.SourceInformation, true));
                                    break;

                                 case 'C':
                                    TDBCopyrightCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.CopyrightCodes.CopyrightInformation, true));
                                    break;

                                 case '*':
                                    TDBCopyrightCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.CopyrightCodes.Unknown, true));
                                    break;

                                 default:
                                    throw new Exception("Falsche Angabe in der Copyright-Option: '" + arg[0] + "'");
                              }

                              switch (arg[1]) {
                                 case 'I':
                                    TDBCopyrightWhereCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.WhereCodes.ProductInformation, true));
                                    break;

                                 case 'P':
                                    TDBCopyrightWhereCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.WhereCodes.Printing, true));
                                    break;

                                 case 'E':
                                    TDBCopyrightWhereCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.WhereCodes.ProductInformationAndPrinting, true));
                                    break;

                                 case '*':
                                    TDBCopyrightWhereCodes.Add(new Property((int)GarminCore.Files.File_TDB.SegmentedCopyright.Segment.WhereCodes.Unknown, true));
                                    break;

                                 default:
                                    throw new Exception("Falsche Angabe in der Copyright-Option: '" + arg[1] + "'");
                              }

                              switch (arg[2]) {
                                 case 'N':
                                    string sText = arg.Substring(3).Trim();
                                    if (sText.Length >= 2)
                                       if (sText[0] == '"' && sText[sText.Length - 1] == '"')
                                          sText = sText.Substring(1, sText.Length - 2);
                                    TDBCopyrightText.Add(new Property(sText, true));
                                    break;

                                 case 'D':
                                    TDBCopyrightText.Add(new Property(null, false));
                                    break;

                                 case '*':
                                    TDBCopyrightText.Add(new Property(null, true));
                                    break;

                                 default:
                                    throw new Exception("Falsche Angabe in der Copyright-Option: '" + arg[1] + "'");
                              }
                           }
                        }
                        break;

                     case MyOptions.SetDescription:
                        Description.Set(cmd.StringValue((int)opt));
                        break;

                     case MyOptions.SetTransparent:
                        Transparent.Set(cmd.BooleanValue((int)opt) ? 1 : 0);
                        break;

                     case MyOptions.SetPriority:
                        Priority.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetMapFamilyName:
                        MapFamilyName.Set(cmd.StringValue((int)opt));
                        break;

                     case MyOptions.SetMapSeriesName:
                        MapSeriesName.Set(cmd.StringValue((int)opt));
                        break;

                     case MyOptions.SetVersion:
                        Version.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetRoutable:
                        Routable.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetHighestRoutable:
                        HighestRoutable.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetHasDEM:
                        HasDEM.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetHasProfile:
                        HasProfile.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.SetMaxCoordBits4Overview:
                        MaxCoordBits4Overview.Set((int)cmd.UnsignedIntegerValue((int)opt));
                        break;

                     case MyOptions.RefreshTDB:
                        ToDo = ToDoType.RefreshTDB;
                        break;

                     case MyOptions.NewTypfile:
                        NewTypfile.Set(cmd.StringValue((int)opt));
                        ToDo = ToDoType.SetNewTypfile;
                        break;

                     case MyOptions.Help:
                        ShowHelp();
                        break;

                  }
            }

            //TestParameter = new string[cmd.Parameters.Count];
            //cmd.Parameters.CopyTo(TestParameter);

            if (cmd.Parameters.Count > 0)
               throw new Exception("Es sind keine Argumente sondern nur Optionen erlaubt.");

            Input = new string[InputArray_Tmp.Count];
            InputArray_Tmp.CopyTo(Input);

         } catch (Exception ex) {
            Console.Error.WriteLine(ex.Message);
            ShowHelp();
            throw new Exception("Fehler beim Ermitteln oder Anwenden der Programmoptionen.");
         }
      }

      void InterpretTypes(string arg, SortedSet<int> types) {
         int pos = arg.IndexOf(':');
         if (pos >= 0) {
            string[] subargs = arg.Substring(pos + 1).Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (types == null)
               types = new SortedSet<int>();
            for (int j = 0; j < subargs.Length; j++) {
               if (!FSoftUtils.CmdlineOptions.IntegerIsPossible(subargs[j], out int type))
                  throw new Exception("Die Typangabe '" + subargs[j] + "' in '" + arg + "' kann nicht interpretiert werden.");
               if (!types.Contains(type))
                  types.Add(type);
            }
         }
      }

      /// <summary>
      /// interpretiert einen Text als Zahl
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      uint InterpretUInt(string txt) {
         int pos = txt.IndexOf(':');
         if (pos < 0 || !FSoftUtils.CmdlineOptions.UnsignedIntegerIsPossible(txt.Trim().Substring(pos + 1), out uint val))
            throw new Exception("'" + txt + "' kann nicht als Zahl interpretiert werden.");
         return val;
      }

      /// <summary>
      /// interpretiert einen Text als logischen Wert
      /// </summary>
      /// <param name="txt"></param>
      /// <returns></returns>
      bool InterpretBool(string txt) {
         int pos = txt.IndexOf(':');
         if (pos < 0 || !FSoftUtils.CmdlineOptions.BooleanIsPossible(txt.Trim().Substring(pos + 1), out bool val))
            throw new Exception("'" + txt + "' kann nicht als logischer Wert interpretiert werden.");
         return val;
      }

      /// <summary>
      /// Hilfetext für Optionen ausgeben
      /// </summary>
      /// <param name="cmd"></param>
      public void ShowHelp() {
         List<string> help = cmd.GetHelpText();
         for (int i = 0; i < help.Count; i++) Console.Error.WriteLine(help[i]);
         Console.Error.WriteLine();
         Console.Error.WriteLine("Zusatzinfos:");


         Console.Error.WriteLine("Für '--' darf auch '/' stehen und für '=' auch ':' oder Leerzeichen.");
         Console.Error.WriteLine("Argumente mit ';' werden an diesen Stellen in Einzelargumente aufgetrennt.");

         // ...

      }


   }
}
