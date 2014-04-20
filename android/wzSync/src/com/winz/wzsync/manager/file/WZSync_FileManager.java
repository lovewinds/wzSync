package com.winz.wzsync.manager.file;

import java.io.BufferedWriter;
import java.io.File;
import java.io.FileOutputStream;
import java.io.FileWriter;

import android.os.Build;
import android.os.Environment;
import android.util.Log;

public class WZSync_FileManager {
	public boolean WriteFile(/*InputStream is*/)
	{
		String state = Environment.getExternalStorageState();

		if( state.compareTo(Environment.MEDIA_MOUNTED)==0 )
		{
			int SDK_INT = android.os.Build.VERSION.SDK_INT;
			if( SDK_INT > Build.VERSION_CODES.FROYO )
			{
				Log.d("wzSync", "[FileMgr] Path : "+
						Environment.getExternalStoragePublicDirectory(Environment.DIRECTORY_PICTURES));
			}
			Log.d("wzSync", "[FileMgr] Path-o : "+
					Environment.getExternalStorageDirectory() );
			
			CreateFile( Environment.getExternalStorageDirectory() );
			
		}
		else
		{
			Log.d("wzSync", "[FileMgr] Can't access to sdcard !!");
		}
		//MEDIA_MOUNTED_READ_ONLY
		
		return false;
	}
	
	private void CreateFile(File path)
	{
		try
		{
			File file = new File( path+File.separator+"wzSync.log" );
			Log.d("wzSync","[FileMgr] Trying to create file : "+path+File.separator+"wzSync.log");
			if( file.exists() )
			{
				Log.d("wzSync","[FileMgr] Already exists file! ");
			}
			else
			{
				if( file.createNewFile() )
				{
					
					FileWriter fw = new FileWriter(file);
					BufferedWriter bfw = new BufferedWriter(fw);
					bfw.write("wzSync Test file");
					bfw.flush();
					bfw.close();
					
					//fw.flush();
					fw.close();
					//FileOutputStream fos = new FileOutputStream(파일명);
				}
				else
				{
					Log.d("wzSync","[FileMgr] Can't create file on : "+path);
				}
			}
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}
}