using System.Collections.Generic;
using System;
using System.Text;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace MixchSitePlugin
{
    class Item
    {
        private bool initialized = false;
        private Dictionary<int, string> m = new Dictionary<int, string>() { };

        public async Task ReadItems(LiveUrlInfo liveUrlInfo)
        {
            // 何度も読み込まないように読み込み済みかどうかチェックする
            if (initialized) {
                return;
            }
            initialized = true;

            // まずはローカルファイルから読み込む
            var path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"settings\mixch_item.txt");
            using (var reader = new System.IO.StreamReader(path, Encoding.GetEncoding("UTF-8")))
            {
                while (reader.Peek() >= 0)
                {
                    var line = reader.ReadLine();
                    string[] cols = line.Split('=');
                    if (cols.Length == 2)
                    {
                        try
                        {
                            var id = int.Parse(cols[0]);
                            m[id] = cols[1];
                            Debug.WriteLine("ReadItemSuccess: " + id + "=" + cols[1]);
                        }
                        catch (FormatException e)
                        {
                            Debug.WriteLine("ReadItemError: " + e);
                        }
                    }
                    else
                    {
                        Debug.WriteLine("ReadItemIgnore: " + line);
                    }
                }
            }

            // サーバーからアイテムリストを取得する
            var client = new HttpClient();
            var url = @"https://mixch.tv/v5/live/item/list";
            Debug.WriteLine($"Environment = {liveUrlInfo.Environment}");
            if (liveUrlInfo.Environment != "torte")
            {
                url = @$"https://{liveUrlInfo.Environment}.mixch.tv/v5/live/item/list";
            }
            Debug.WriteLine($"Item get url = {url}");
            var result = await client.GetAsync(url);

            // ステータスコードが 200 以外ならアイテムリストの処理を終了する
            if (result.StatusCode != System.Net.HttpStatusCode.OK)
            {
                Debug.WriteLine($"StatusFailed = {(int)result.StatusCode}");
                return;
            }
            var content = await result.Content.ReadAsStringAsync();
            MixchItemList mixchItemList;
            mixchItemList = JsonConvert.DeserializeObject<MixchItemList>(content);

            // データが取得できていなければアイテムリストの処理を終了する
            if (mixchItemList.mixchItems == null)
            {
                Debug.WriteLine("mixchItemList.mixchItems is null");
                return;
            }

            foreach (MixchItem i in mixchItemList.mixchItems)
            {
                if (!m.ContainsKey(i.id))
                {
                    // アイテムファイルにないIDのアイテムをセットする
                    m[i.id] = i.name;
                    Debug.WriteLine("SetItem ID: " + i.id + ", name: " + i.name);
                }
                else {
                    Debug.WriteLine("DiscardItem ID: " + i.id + ", name: " + i.name);
                }
            }
        }

        public string NameByResourceId(int id)
        {
            return m.ContainsKey(id) ? m[id] : "";
        }

        public class MixchItemList {

            [JsonProperty("items")]
            public MixchItem[] mixchItems { get; set; }
        }

        public class MixchItem
        {
            [JsonProperty("id")]
            public int id { get; set; }

            [JsonProperty("name")]
            public string name { get; set; }
        }
    }
}
