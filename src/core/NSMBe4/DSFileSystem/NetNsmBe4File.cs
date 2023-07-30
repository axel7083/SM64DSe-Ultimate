namespace SM64DSe.core.NSMBe4.DSFileSystem
{
    public class NetNsmBe4File : NsmBe4FileWithLock
    {
        NetFilesystem netfs;
        public NetNsmBe4File(NetFilesystem parent, NSMBe4Directory parentDir, string name, int id, int ffileSize)
            : base(parent, parentDir, name, id)
        {
            this.netfs = parent;
            this.fileSizeP = ffileSize;
        }


        public override byte[] getContents()
        {
            NSMBe4ByteArrayOutputStream request = new NSMBe4ByteArrayOutputStream();
            request.writeInt(id);
            return netfs.doRequest(2, request.getArray(), this);
        }

        public override byte[] getInterval(int start, int end)
        {
            NSMBe4ByteArrayOutputStream request = new NSMBe4ByteArrayOutputStream();
            request.writeInt(id);
            request.writeInt(start);
            request.writeInt(end);
            return netfs.doRequest(3, request.getArray(), this);
        }

        public override void startEdition()
        {
            NSMBe4ByteArrayOutputStream request = new NSMBe4ByteArrayOutputStream();
            request.writeInt(id);
            netfs.doRequest(4, request.getArray(), this);
        }

        public override void endEdition()
        {
            NSMBe4ByteArrayOutputStream request = new NSMBe4ByteArrayOutputStream();
            request.writeInt(id);
            netfs.doRequest(5, request.getArray(), this);
        }

        public override void replace(byte[] newFile, object editor)
        {
            NSMBe4ByteArrayOutputStream request = new NSMBe4ByteArrayOutputStream();
            request.writeInt(id);
            request.writeInt(newFile.Length);
            request.write(newFile);
            netfs.doRequest(6, request.getArray(), this);
        }
        public override void replaceInterval(byte[] newFile, int start)
        {
            NSMBe4ByteArrayOutputStream request = new NSMBe4ByteArrayOutputStream();
            request.writeInt(id);
            request.writeInt(start);
            request.writeInt(start + newFile.Length);
            request.write(newFile);
            netfs.doRequest(7, request.getArray(), this);
        }
    }
}
