using System;
using System.Net.Sockets;

namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public class NetFilesystem : Filesystem
    {
        string host;
        int port;
        TcpClient net;
        NetworkStream stream;

        public NetFilesystem(string host, int port)
        {
            this.host = host;
            this.port = port;

            net = new TcpClient(host, port);
            stream = net.GetStream();

            Console.WriteLine("Connected to " + host + ":" + port);

            byte[] data = doRequest(1);
            mainDir = loadDir(new NSMBe4ByteArrayInputStream(data), null);
        }


        public override string getRomPath()
        {
            return host+":"+port;
        }

        private NSMBe4Directory loadDir(NSMBe4ByteArrayInputStream s, NSMBe4Directory parent)
        {
            int id = s.readInt();
            string name = s.ReadString();

            NSMBe4Directory d = new NSMBe4Directory(this, parent, false, name, id);
            addDir(d);
            if(parent != null) parent.childrenDirs.Add(d);

            int dirCount = s.readInt();
            for (int i = 0; i < dirCount; i++)
                d.childrenDirs.Add(loadDir(s, d));
            int fileCount = s.readInt();
            for (int i = 0; i < fileCount; i++)
            {
                int fid = s.readInt();
                int fsize = s.readInt();
                string fname = s.ReadString();
                NetNsmBe4File f = new NetNsmBe4File(this, d, fname, fid, fsize);
                d.childrenFiles.Add(f);
                addFile(f);
            }
            return d;
        }
        

        public byte[] doRequest(byte type, byte[] data = null, NSMBe4File f = null)
        {
            NSMBe4ByteArrayOutputStream bout = new NSMBe4ByteArrayOutputStream();
            if (data == null)
            {
                bout.writeInt(1);
                bout.writeByte(type);
            }
            else
            {
                bout.writeInt(data.Length+1);
                bout.writeByte(type);
                bout.write(data);
            }
            byte[] aout = bout.getArray();
            stream.Write(aout, 0, aout.Length);
            stream.Flush();

            byte[] resp = readBytes(8);
            NSMBe4ByteArrayInputStream bin = new NSMBe4ByteArrayInputStream(resp);
            int error = bin.readInt();
            int len = bin.readInt();
            resp = readBytes(len);

            if (error == 2) throw new AlreadyEditingException(f);
            if (error != 0)
            {
                NSMBe4ByteArrayInputStream i = new NSMBe4ByteArrayInputStream(resp);
                string s = i.ReadString();
                throw new Exception("Network error: " + s);
            }

            return resp;
        }

        private byte[] readBytes(int len)
        {
            int i = 0;
            byte[] res = new byte[len];
            while (i != len)
            {
                int read = stream.Read(res, i, len - i);
                i += read;
                if (read == 0)
                    throw new Exception("Wat");
            }
            return res;
        }
    }
}
