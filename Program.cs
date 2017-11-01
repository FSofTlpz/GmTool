using System;
using System.Reflection;

/*
Information about files (-i):
  gmt -i [-v] file...

	-v - verbose

Join maps (-j):
  gmt -j [-v] [-i] [-a] [-b block] [-c no.no[,ms[,prod]]] [-d] [-f FID[,PID]]
         [-h] [-l|-n] [-m map] [-o output_file] [-q] [-r] [-u code]
         [-x] [-z] file...

	-i - information
	-v - verbose

	-a - use other data
	-b - block size kB
	-c - map version, Mapsource flag, product code in header
	-d - create DEMO map
	-f - Family ID and Product ID
	-h - short header img
	-l - use name BLUCHART.MPS
	-m - mapset name
	-n - use name MAPSOURC.MPS
	-o - output file name
	-q - map without autorouting and DEM data
	-r - remove unlock codes
	-u - new unlock code
	-x - do not create MPS subfile
	-z - convert int NT-like format

Split maps for Mapsource (-S):
  gmt -S [-v] [-i] [-c CodePage] [-f FID[,PID]] [-h] [-m map] [-L]
         [-l] [-n name] [-o path] [-q] [-r] [-t] [-3] file...

	-i - information
	-v - verbose

	-c - CodePage for mapset
	-f - Family ID and Product ID
	-h - short header img
	-L - limit map longitude to 178.5
	-l - limit longitude in preview map to 178.5
	-m - mapset name
	-n - mapset files name
	-o - output path
	-q - add empty DEM
	-r - create TDB for marine map
	-t - create split.lst
	-3 - create TDB version 3, if possible

Split maps (-s):
  gmt -s [-v] [-i] [-h] [-m map] [-o path] [-t] [-x] file.img...

	-i - information
	-v - verbose

	-h - short header img
	-m - mapset name
	-o - output path
	-t - create split.lst
	-x - save TYP files only

Split mapsets by FID(-G):
  gmt -G [-v] [-i] [-h] [-l|-n] [-o path] file.img...

	-i - information
	-v - verbose

	-h - short header img
	-l - use name BLUCHART.MPS
	-n - use name MAPSOURC.MPS
	-o - output path

Split into subfiles (-g):
  gmt -g [-v] [-i] [-o path] [-t] file.img...

	-i - information
	-v - verbose

	-o - output path
	-t - create split.lst

Split into empty maps (-k):
  gmt -k [-v] [-i] [-o path] file.img...

	-i - information
	-v - verbose

	-o - output path
 
Write changes into oryginal files (-w):
  gmt -w [-i] [-v] [-c no.no[,ms[,prod]]] [-e [+|-]map_id] [-f FID[,PID]] 
         [-h] [-L|-l|-1] [-m map] [-n|-t] [-q t1,t2,t3[,t4]] [-p priority]
         [-r name] [-u code] [-x] [-y FID[,PID[,CP]]] [-r FID[,PID]] file...

	-i - information
	-v - verbose

	-c - map version, mapsource flag, product code in header
	-e - new map ID number
	-f - Family ID and Product ID
	-h - refresh header date
	-L - upper case labels
	-l - lower case labels
	-m - mapset name
	-n - non-transparent map
	-p - map priority
	-q - parameters TRE
	-r - map name
	-t - transparent map
	-u - new unlock code
	-x - repleace TYP file in img file
	-y - correct TYP Family ID, Product ID and CodePage
	-z - change Family ID, Product ID in *.MPS
	-1 - first character upper case in labels

You can provide input file list using option -@ list.txt.

Use as first option -LicenseAcknowledge to remove license message.


	-f - Family ID and Product ID
	-t - transparent map
	-p - map priority
	-r - map name
	-m - mapset name

 */


namespace GmTool {
   class Program {

      static void Main(string[] args) {
         Options opt = new Options();
         try {

            Assembly a = Assembly.GetExecutingAssembly();
            Console.WriteLine(
               ((AssemblyProductAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyProductAttribute)))).Product + ", Version vom " +
               ((AssemblyInformationalVersionAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyInformationalVersionAttribute)))).InformationalVersion + ", " +
               ((AssemblyCopyrightAttribute)(Attribute.GetCustomAttribute(a, typeof(AssemblyCopyrightAttribute)))).Copyright
            );
            Console.WriteLine("   Clipper Library 6.4.0 von Angus Johnson (http://sourceforge.net/projects/polyclipping)");
            Console.WriteLine(GarminCore.Garmin.DllTitle());
            Console.WriteLine();

            opt.Evaluate(args);
         } catch (Exception ex) {
            Console.Error.WriteLine("Fehler beim Ermitteln der Programmoptionen: " + ex.Message);
            opt = null;
         }

         if (opt != null) {
            TheJob job = new TheJob();
            job.Run(opt);
         }

      }
   }
}
