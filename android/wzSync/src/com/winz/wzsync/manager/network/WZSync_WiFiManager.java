package com.winz.wzsync.manager.network;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.net.wifi.WifiInfo;
import android.net.wifi.WifiManager;
import android.util.Log;
import android.widget.Toast;

import com.winz.wzsync.manager.file.WZSync_FileManager;

public class WZSync_WiFiManager extends BroadcastReceiver {
	private WZSync_FileManager fileManager = new WZSync_FileManager();
	
	@Override
    public void onReceive(final Context context, final Intent intent) {
 
        String status = isConnected(context);
        if( status != null )
        	Toast.makeText(context, status, Toast.LENGTH_SHORT).show();
        else
        	Toast.makeText(context, "Not connected yet.", Toast.LENGTH_SHORT).show();
    }

	
	public String isConnected(Context ctx)
	{
		String result = null;
		
		WifiManager wManager;
		wManager = (WifiManager)ctx.getSystemService(Context.WIFI_SERVICE);
		WifiInfo wInfo = wManager.getConnectionInfo();      
		
		// 에뮬에서 테스트 위해서 WiFi 체크 주석처리
		if ( wManager.isWifiEnabled() == true && wInfo.getSSID() != null)
		{   		
			Log.d("wifi", "WiFi is Enabled");
			Log.d("wifi", "SSID : " + wInfo.getSSID() );
			result = wInfo.getSSID();
			if( result.compareTo("\"miroonamu\"")==0)
			{
				Log.d("wzSync", "I'm here!");
				fileManager.WriteFile();
			}
		}
		else
		{
			Log.d("wifi", "WiFi is disabled");
			Log.d("wifi", "SSID : " + wInfo.getSSID() );
		}
		
		return result;
	}
}
