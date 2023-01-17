using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace GenesixInv.Helper
{
	public class GnxHsh
	{
		public GnxHsh()
		{
		}

        public long gethsh(string name, ref string buf, string key, int keylen, int reclen)
        {
            long del = 0, rec, top = 0;
            char flg;
            String tmp;
            int ind, key_len;
            long totrec;
            string filename = Path.Combine(FileSystem.AppDataDirectory, name);
            FileInfo fi = new FileInfo(filename);

            if (!fi.Exists)
                return -1;

            totrec = fi.Length / reclen;

            if (totrec < 8)             /* min. 8 recs/bucket */
                return (-1);
            key_len = keylen;
            if (key_len < 24)
                key_len = 24;
            else
               if ((key_len % 8) != 0)
                key_len = ((key_len / 8) * 8) + 8;
            tmp = key.PadLeft(keylen);
            if (tmp.Length > keylen)
                tmp = tmp.Substring(0,keylen);
            ind = tmp.Length;
            while ((ind--) > 0)
            {
                if ((flg = (char)(tmp[ind] & 15)) > 10)
                    flg = (char)(flg - 6);
                tmp= setChar(tmp,ind, (char)(flg | '0'));
            }
            for (ind = key[1] & 15; (ind--) > 0;)
                tmp = tmp.Substring(1) + tmp[0];
            for (ind = key_len - 8; ind >= 0; ind -= 8)
            {
                if (ind < tmp.Length)
                {
                    top += long.Parse(tmp.Substring(ind));
                    tmp = tmp.Substring(0, ind);
                }
            }
            top %= totrec >> 3;
            top <<= 3;
            BinaryReader b = new BinaryReader(File.Open(filename, FileMode.Open));
            b.BaseStream.Seek( top * reclen,  SeekOrigin.Begin);    /* set file pointer   */
            for (rec = top + 1; ; rec++)                /* incrementing rec-no*/
            {
                var buffer = b.ReadBytes(reclen);
                if (buffer.Length != reclen) /* READ      */
                {
                    b.Close();
                    return (-2);            /* error by reading   */
                }
                buf = Encoding.GetEncoding("windows-1252").GetString(buffer);
                if (buf[keylen - 1] < '0')          /* empty/deleted ?    */
                {
                    if (del != 0)                   /* if no empty rec yet*/
                        del = rec;                  /* save empty rec-no. */
                    if (buf[keylen - 1] == ' ')     /* never used record ?*/
                        break;                  /* then end of search */
                }
                else                        /* used record found  */
                    if (key == buf.Substring(0, keylen))   /* if key equals rec  */
                    {
                        b.Close();
                        return rec;               /* return record-no.  */
                    }
                if (rec == totrec)              /* if end-of-file ?   */
                    b.BaseStream.Seek(rec = 0, SeekOrigin.Begin);     /* set start-of-file  */
                if (rec == top)                 /* cur.rec = 1st.rec ?*/
                    break;                      /* record not found   */
            }

            if (del != rec)                 /* empty rec-no.saved?*/
                b.BaseStream.Seek(del * reclen, SeekOrigin.Begin); /* then position there*/
            b.Close();
            return (del == 0 ? -3 : 0);     /* file full=-3 / not found=0 */
        }

        private string setChar(string tmp, int ind, char v)
        {
            string res = "";
            if (ind > 0)
                res += tmp.Substring(0, ind);
            res += v;
            if (ind < (tmp.Length - 1))
                res += tmp.Substring(ind + 1);
            return res;
        }

        private static async Task SaveFile(string fileUrl, string pathToSave)
        {
            // See https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
            // for why, in the real world, you want to use a shared instance of HttpClient
            // rather than creating a new one for each request
            var httpClient = new HttpClient();

            var httpResult = await httpClient.GetAsync(fileUrl);
            var resultStream = await httpResult.Content.ReadAsStreamAsync();
            var fileStream = File.Create(pathToSave);
            resultStream.CopyTo(fileStream);
            fileStream.Close();
        }
        public static async  Task<bool> downloadArticulos()
        {
            try
            {
                await SaveFile(App.GnxSettings.urlarticulos, Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "articulos.zip"));
                FileInfo fi1 = new FileInfo(Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "articulos.zip"));
                File.Delete(Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "articulos.hsh"));
                ZipFile.ExtractToDirectory(Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "articulos.zip"), FileSystem.AppDataDirectory);
                FileInfo fi = new FileInfo(Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "articulos.hsh"));
                return true;

            }
            catch (Exception e)
            {
                var msg = e.Message;

            }

            return false;
        }
    }
}

