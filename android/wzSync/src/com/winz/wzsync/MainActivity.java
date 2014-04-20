package com.winz.wzsync;

import android.app.Activity;
import android.app.Fragment;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.ViewGroup;

import com.winz.wzsync.manager.file.WZSync_FileManager;
import com.winz.wzsync.manager.network.WZSync_NetworkManager;
import com.winz.wzsync.manager.network.WZSync_WiFiManager;

public class MainActivity extends Activity {
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);
		
		if (savedInstanceState == null) {
			getFragmentManager().beginTransaction()
					.add(R.id.container, new PlaceholderFragment()).commit();
		}
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {

		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.main, menu);
		return true;
	}

	@Override
	public boolean onOptionsItemSelected(MenuItem item) {
		// Handle action bar item clicks here. The action bar will
		// automatically handle clicks on the Home/Up button, so long
		// as you specify a parent activity in AndroidManifest.xml.
		int id = item.getItemId();
		if (id == R.id.action_settings) {
			return true;
		}
		return super.onOptionsItemSelected(item);
	}

	/**
	 * A placeholder fragment containing a simple view.
	 */
	public static class PlaceholderFragment extends Fragment implements OnClickListener {

		public PlaceholderFragment() {
		}

		@Override
		public View onCreateView(LayoutInflater inflater, ViewGroup container,
				Bundle savedInstanceState) {
			View rootView = inflater.inflate(R.layout.fragment_main, container,
					false);
			
			rootView.findViewById(R.id.button_test).setOnClickListener(this);
			rootView.findViewById(R.id.button_connect).setOnClickListener(this);
			/*
			findViewById(R.id.button_test).setOnClickListener(
					new Button.OnClickListener() {
						public void onClick(View v) {
							Toast.makeText(getApplicationContext(), "Test", 3).show();
						}
					});
			*/
			return rootView;
		}
		
		public void onClick( View v ) {
			WZSync_NetworkManager net = WZSync_NetworkManager.getInstance();
			
			switch(v.getId())
			{
			case R.id.button_test:
				net.sendMessage();
				break;
				
			case R.id.button_connect:
				WZSync_WiFiManager wifi = new WZSync_WiFiManager();
				boolean status = wifi.isConnected(getActivity().getApplicationContext(), null);
				if( status == true )
				{
					net.connect();
				}
				//NetworkManager.getInstance().connect();
				//Toast.makeText(getActivity(), "Test", Toast.LENGTH_SHORT).show();
				break;
			}
		}
	}

}