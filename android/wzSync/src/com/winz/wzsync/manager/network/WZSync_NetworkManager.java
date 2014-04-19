package com.winz.wzsync.manager.network;

import java.io.ByteArrayOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.Socket;
import java.net.UnknownHostException;

import android.os.AsyncTask;
import android.util.Log;

import com.winz.wzsync.manager.file.WZSync_FileManager;

public class WZSync_NetworkManager extends AsyncTask<Void, Void, Void>
{
	// Managers
	private static WZSync_NetworkManager instance = null;
	private WZSync_FileManager fileManager = null;
	
	private final String ServIP = "192.168.0.10";
	private final int ServPort = 3750;
	private Socket clientSocket = null;
	
	private WZSync_NetworkManager()
	{
		fileManager = new WZSync_FileManager();
	}
	
	public static WZSync_NetworkManager getInstance()
	{
		if( instance == null )
		{
			instance = new WZSync_NetworkManager();
		}
		return instance;
	}
	
	public void connect()
	{
		if( clientSocket.isConnected() == true )
		{
			Log.d("wzSync", "Already Connected.");
		}
		else
		{
			Log.d("wzSync", "Trying to connect server...");
			instance.execute();
		}
	}

	@Override
	public Void doInBackground(Void... arg0) {
		// TODO 자동 생성된 메소드 스텁
		try
		{
			clientSocket = new Socket(ServIP, ServPort);
			InputStream in = clientSocket.getInputStream();
			ByteArrayOutputStream byteArrayOutputStream = new ByteArrayOutputStream(1024);
			
			byte[] buffer = new byte[1024];
			
			int bytesRead = 0;
			int cnt = 1;
			while( (bytesRead=in.read(buffer)) > 0 )
			{
				byteArrayOutputStream.write(buffer, 0, bytesRead);
				Log.d("wzSync",
						String.format("[%d] : %s", cnt++,  byteArrayOutputStream.toString("UTF-8")));
				
			}
			clientSocket.close();
			
		}
		catch( UnknownHostException e ) {
			e.printStackTrace();
		}
		catch( IOException e ) {
			e.printStackTrace();
		}
		return null;
	}
	
	@Override
	protected void onPostExecute(Void result) {
		//Toast.makeText(, text, duration)
		
	}
}