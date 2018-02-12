# gmtool
This is an experimental tool for garmin maps. You can join and split files, getting infos about garmin files and so on.

few examples:

show all known infos for a file:
      
      gmtool -i 70260019.img -I 99
   
show minimal infos for a file:
      
      gmtool -i osmmap.tdb -I

split a IMG to a subdirectory with dec number of ID (1881538585):
      
      gmtool -i 70260019.img --split -o .
   
      1881538585\70260019.DEM
      1881538585\70260019.LBL
      1881538585\70260019.NET
      1881538585\70260019.NOD
      1881538585\70260019.RGN
      1881538585\70260019.TRE

join files to IMG:
      
      gmtool -i 1881538585\*.* --join=tile -o 70260019new.img

join IMG, MDX and TYP files to IMG for gps:
   
      gmtool -i *.img -i *.mdx -i *.typ --join=device -o gps.img

But caution, a file like osmmap_mdr.img (only for mapsource) must not be in the same directory. Move before temporarly to another directory.

set props like description:

      gmtool -i 70260019.img --description="new description"
      gmtool -i osmmap.tdb --description="new description"

create a new TDB osmmap.tdb with infos from the files in the directory:

      gmtool -i . --mapsource=ov:osmmap.img;tdb:osmmap.tdb;noov;notyp;nomdx;nomdr;noinst --mapfamilyname="Family" --mapseriesname="Series" --description="Description" --routable=1 --highestroutable=24 --maxbits4overview=18 --hasdem=1 --hasprofile=1 --copyright=*I*  -o . --overwrite

There is no way to find the overviewmap automaticly. Because we set the name with --mapsource=ov:...

create a new TDB new.tdb with infos from files in the directory and an old.tdb and additional setting of DEM-Property:

      REM for a lot of IMG's with TDB:
      gmtool -i old.tdb -i . --mapsource=tdb:new.tdb;noov;notyp;nomdx;nomdr;noinst --hasdem=1 -o . --overwrite
   
      REM for gmap-style maps with TDB in the Product1-directory:
      gmtool -i old.tdb -i . --withsubdirs --mapsource=tdb:new.tdb;noov;notyp;nomdx;nomdr;noinst --hasdem=1 -o . --overwrite
      
refresh a existing TDB (filesize, new files, ...):
   
      gmtool -i my.tdb --refreshtdb
