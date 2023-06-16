﻿using ConsoleTesting.Functions;
using PrivateBinSharp.Crypto.crypto.digests;
using PrivateBinSharp.Crypto.crypto.engines;
using PrivateBinSharp.Crypto.crypto.generators;
using PrivateBinSharp.Crypto.crypto.modes;
using PrivateBinSharp.Crypto.crypto.parameters;
using PrivateBinSharp.Crypto.security;
using System.Net.NetworkInformation;
using System.Text;

namespace PrivateBinSharp
{
    public class PrivateBinClient
    {
        public PrivateBinClient(string hostUrl)
        {
            HostUrl = hostUrl;
            Http = new HttpClient();
            Http.BaseAddress = new Uri(HostUrl);
            Http.DefaultRequestHeaders.Add("X-Requested-With", "JSONHttpRequest");
        }

        public string HostUrl { get; internal set; }

        private bool FirstTimeCheck;

        private HttpClient Http;

        public async Task<string> CreatePaste(string text, string password, string expire = "5min")
        {
            if (FirstTimeCheck)
            {
                try
                {
                    HttpResponseMessage TestRes = await Http.GetAsync(HostUrl);
                    TestRes.EnsureSuccessStatusCode();
                    FirstTimeCheck = false;
                }
                catch
                {
                    return string.Empty;
                }
            }
            Tuple<PasteJson, byte[]> Json = await GeneratePasteData(text, password, expire);
            string body = Newtonsoft.Json.JsonConvert.SerializeObject(Json.Item1);
            HttpRequestMessage Req = new HttpRequestMessage(HttpMethod.Post, HostUrl)
            {
                Content = new StringContent(body, Encoding.UTF8)
            };
            Req.Headers.Add("X-Requested-With", "JSONHttpRequest");
            HttpResponseMessage Res = await new HttpClient().SendAsync(Req);
            string response = await Res.Content.ReadAsStringAsync();
            Console.WriteLine("Raw: " + response);
            PasteResponse? responseJson = Newtonsoft.Json.JsonConvert.DeserializeObject<PasteResponse>(response);

            return responseJson.id + '#' + Base58.EncodePlain(Json.Item2);
        }

        private async Task<Tuple<PasteJson, byte[]>> GeneratePasteData(string text, string password, string expire)
        {
            SecureRandom rng = new();

            string pasteDataJson = Newtonsoft.Json.JsonConvert.SerializeObject(new PasteBlobJson
            {
                paste = text
            });
            byte[] pasteBlob = Encoding.UTF8.GetBytes(pasteDataJson);
            byte[] _pastePassword = new byte[0];
            byte[] urlSecret = new byte[32];
            rng.NextBytes(urlSecret);
            byte[] pastePassphrase = urlSecret;
            int kdfIterations = 100000;
            int kdfKeysize = 32;
            byte[] kdfSalt = new byte[8];
            rng.NextBytes(kdfSalt);
            Pkcs5S2ParametersGenerator pdb = new(new Sha256Digest());
            pdb.Init(pastePassphrase, kdfSalt, kdfIterations);
            byte[] kdfKey = (pdb.GenerateDerivedMacParameters(256) as KeyParameter).GetKey();
            int nonceSize = 12;
            byte[] cipherIv = new byte[nonceSize];
            rng.NextBytes(cipherIv);
            string cipherAlgo = "aes";
            string cipherMode = "gcm";
            int cipherTagSize = 128;
            string compressionType = "none";
            int _openDiscussion = 0;
            int _burnAfterReading = 0;

            object[] pasteMetaObj = new object[]
            {
                new object[]
                {
                    Convert.ToBase64String(cipherIv),
                    Convert.ToBase64String(kdfSalt),
                    kdfIterations,
                    256,
                    cipherTagSize,
                    cipherAlgo,
                    cipherMode,
                    compressionType
                },
                "plaintext",
                _openDiscussion,
                _burnAfterReading
            };
            string pasteMetaJson = Newtonsoft.Json.JsonConvert.SerializeObject(pasteMetaObj);
            byte[] pasteMeta = Encoding.UTF8.GetBytes(pasteMetaJson);

            GcmBlockCipher cipher = new(new AesEngine());
            AeadParameters parameters = new AeadParameters(
                new KeyParameter(kdfKey), cipherTagSize, cipherIv, pasteMeta);
            cipher.Init(true, parameters);
            byte[] cipherText = new byte[cipher.GetOutputSize(pasteBlob.Length)];
            int len = cipher.ProcessBytes(pasteBlob, 0, pasteBlob.Length, cipherText, 0);
            cipher.DoFinal(cipherText, len);

            return new Tuple<PasteJson, byte[]>(new PasteJson(expire, cipherText, pasteMetaObj), urlSecret);
        }
    }

    internal class PasteBlobJson
    {
        public string paste;
    }

    internal class PasteResponse
    {
        public string id;
        public string deletetoken;
    }

    internal class PasteJson
    {
        public PasteJson(string expire, byte[] ciperText, object[] data)
        {
            ct = Convert.ToBase64String(ciperText);
            meta = new
            {
                expire
            };
            adata = data;
        }
        public object[] adata;
        public int v = 2;
        public object meta;
        public string ct;
    }
}