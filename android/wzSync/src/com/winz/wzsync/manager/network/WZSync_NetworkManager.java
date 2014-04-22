package com.winz.wzsync.manager.network;

import java.io.IOException;
import java.net.Inet4Address;
import java.net.InetAddress;
import java.net.InetSocketAddress;
import java.nio.ByteBuffer;
import java.nio.channels.ClosedChannelException;
import java.nio.channels.SelectionKey;
import java.nio.channels.Selector;
import java.nio.channels.SocketChannel;
import java.util.HashMap;
import java.util.Iterator;
import java.util.concurrent.LinkedBlockingQueue;

import android.util.Log;

import com.winz.wzsync.manager.file.WZSync_FileManager;

public class WZSync_NetworkManager extends Thread
{
	// Managers
	private static WZSync_NetworkManager instance = null;
	private WZSync_FileManager fileManager = null;
	
	private final String ServIP = "192.168.0.10";
	private final int ServPort = 3750;
	private boolean isRunning = false;
	
	// Network
	private LinkedBlockingQueue<byte[]> dataQueue = new LinkedBlockingQueue<byte[]>();
    private HashMap<String, Integer> uuidToSize = new HashMap<String, Integer>();
    private Selector selector = null;
	private SocketChannel clientChannel = null;

    
	private WZSync_NetworkManager()
	{
		fileManager = new WZSync_FileManager();
	}
	
	public void finalize()
	{
		try
		{
			
		}
		catch(Exception e )
		{
			e.printStackTrace();
		}
	}
	
	public static WZSync_NetworkManager getInstance()
	{
		if( instance == null )
		{
			instance = new WZSync_NetworkManager();
			//instance.setDaemon(true);
		}
		return instance;
	}
	
	public void connect()
	{
		if(isRunning == false)
		{
			instance.start();
			
			Log.d("wzSync", "Network thread start.");
		}
		else
		{
			Log.d("wzSync", "Already thread running.");
		}
	}
	
	public void sendMessage()
	{
		try
		{
			Thread t = new Thread(new Runnable(){
				public void run() {
					// TODO 자동 생성된 메소드 스텁
					if(selector != null)
					{
						try {
							String str = "Hello? I'm android.";
							SelectionKey key;
							key = clientChannel.register(selector, SelectionKey.OP_WRITE);
							key.attach(str.getBytes("UTF-8"));
							Write(ByteBuffer.allocateDirect(1024), key, selector);
						} catch (Exception e) {
							// TODO 자동 생성된 catch 블록
							e.printStackTrace();
						}
					}					
				}
			});
			t.start();
		}
		catch(Exception e)
		{
			e.printStackTrace();
		}
	}
	
	public void run()
	{
		try
		{
			ByteBuffer buffer = ByteBuffer.allocateDirect(4096);
			
			clientChannel = SocketChannel.open();
			clientChannel.connect(new InetSocketAddress(InetAddress.getByName(ServIP), ServPort));
			
			selector = Selector.open();
			clientChannel.configureBlocking(false);
			clientChannel.register(selector, SelectionKey.OP_CONNECT);
			
			isRunning = true;
			while (selector.isOpen())
			{
                int count = selector.select(10);
                if (count == 0) {
                    continue;
                }

                Iterator<SelectionKey> it = selector.selectedKeys().iterator();
                while (it.hasNext()) {
                    final SelectionKey key = it.next();
                    it.remove();
                    if (!key.isValid()) {
                        continue;
                    }

                    if (key.isConnectable()) {
                    	clientChannel = (SocketChannel) key.channel();
                        if (!clientChannel.finishConnect()) {
                            continue;
                        }
                        clientChannel.register(selector, SelectionKey.OP_WRITE);
                    }

                    if (key.isReadable()) {
                        key.interestOps(0);
                        Read(buffer, key, selector);
                    }
                    if (key.isWritable()) {
                        key.interestOps(0);
                        if(key.attachment() == null){
                            key.attach(dataQueue.take());
                        }
                        Write(buffer, key, selector);
                    }
                }
            }
		}
		catch(Exception e)
		{
			Log.d("wzSync", "[Connect] Error : ");
			e.printStackTrace();
		}
		isRunning = false;
		Log.d("wzSync","[Network] Thread end.");
	}
	

    private boolean checkUUID(byte[] data) {
        return uuidToSize.containsKey(new String(data));
    }
    
    private void Write(ByteBuffer buffer, SelectionKey key, Selector selector)
    {
    	SocketChannel sc = (SocketChannel) key.channel();
        byte[] data = (byte[]) key.attachment();
        buffer.clear();
        buffer.put(data);
        buffer.flip();
        int results = 0;
        while (buffer.hasRemaining()) {
            try {
                results = sc.write(buffer);
            } catch (IOException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }

            if (results == 0) {
                buffer.compact();
                buffer.flip();
                data = new byte[buffer.remaining()];
                buffer.get(data);
                key.interestOps(SelectionKey.OP_WRITE);
                key.attach(data);
                Log.d("wzSync","[WRITE] request : "+new String(data));
                selector.wakeup();
                return;
            }
        }

        key.interestOps(SelectionKey.OP_READ);
        key.attach(null);
        selector.wakeup();
    }
    
    private void Read(ByteBuffer buffer, SelectionKey key, Selector selector)
    {
    	SocketChannel sc = (SocketChannel) key.channel();
        buffer.clear();
        byte[] data = (byte[]) key.attachment();
        if (data != null) {
            buffer.put(data);
        }
        int count = 0;
        int readAttempts = 0;
        try {
            while ((count = sc.read(buffer)) > 0) {
                readAttempts++;
                Log.d("wzSync","[READ] : "+new String(buffer.array()));
            }
        } catch (IOException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
        }

        if (count == 0) {
            buffer.flip();
            data = new byte[buffer.limit()];
            buffer.get(data);
            if (checkUUID(data)) {
                key.interestOps(SelectionKey.OP_READ);
                key.attach(data);
            } else {
                //System.out.println("Clinet Read - uuid ~~~~ " + new String(data));
                Log.d("wzSync","[READ] uuid : "+new String(data));
                key.interestOps(SelectionKey.OP_WRITE);
                key.attach(null);
            }
        }

        if (count == -1) {
            try {
                sc.close();
                Log.d("wzSync","[Connection] Closed.");
            } catch (IOException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }
        }
        selector.wakeup();
    }
}