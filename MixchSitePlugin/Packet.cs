﻿using System;
using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace MixchSitePlugin
{
    class Packet
    {
        [JsonProperty("kind")]
        public int Kind { get; set; }

        [JsonProperty("user_id")]
        public int UserId
        {
            get { return IsSystemMessage() || Anonymous == 1 ? 0 : _userId; }
            set { _userId = value; }
        }
        private int _userId;

        [JsonProperty("name")]
        public string Name
        {
            get { return IsSystemMessage() ? "" : _name; }
            set { _name = value; }
        }
        private string _name;

        [JsonProperty("created")]
        public int Created { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("resource_id")]
        public int ResourceId { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("elapsed")]
        public int Elapsed { get; set; }

        [JsonProperty("display_point")]
        public int DisplayPoint { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("anonymous")]
        public int Anonymous { get; set; }

        [JsonProperty("superfan_rank")]
        public int SuperFanRank { get; set; }

        public bool IsSystemMessage()
        {
            switch ((MixchMessageType)Kind)
            {
                case MixchMessageType.Share:
                case MixchMessageType.EnterNewbie:
                case MixchMessageType.EnterLevel:
                case MixchMessageType.Follow:
                case MixchMessageType.EnterFanclub:
                case MixchMessageType.SuperFan:
                    return true;
            }
            return false;
        }

        public bool IsStatus()
        {
            return (MixchMessageType)Kind == MixchMessageType.Status;
        }

        public bool HasMessage(Item i)
        {
            return !string.IsNullOrEmpty(Message(i));
        }

        public string Message(Item i)
        {
            switch ((MixchMessageType)Kind)
            {
                case MixchMessageType.Comment:
                case MixchMessageType.Share:
                case MixchMessageType.EnterNewbie:
                case MixchMessageType.EnterLevel:
                case MixchMessageType.Follow:
                case MixchMessageType.EnterFanclub:
                    return Body;
                case MixchMessageType.SuperFan:
                    return $"{_name}さんが{SuperFanRankName()}のガチファンになりました";
                case MixchMessageType.SuperComment:
                    return $"【スパコメ {ItemName(i)}】{Body}";
                case MixchMessageType.Stamp:
                    return $"【スタンプ】「{ItemName(i)}」で応援しました";
                case MixchMessageType.PoiPoi:
                    return $"【アイテム】{Count}個の「{ItemName(i)}」で応援しました";
                case MixchMessageType.Item:
                case MixchMessageType.CoinBox:
                    return $"【アイテム】「{ItemName(i)}」で応援しました";
            }
            return "";
        }

        public string PoiPoiKey()
        {
            return $"{UserId}_{ResourceId}";
        }

        public string DisplayPointString()
        {
            return String.Format("盛り上がり度: {0:#,0}", DisplayPoint);
        }

        private string ItemName(Item i)
        {
            var name = i.NameByResourceId(ResourceId);
            return !string.IsNullOrEmpty(name) ? name : $"名称不明(id={ResourceId})";
        }

        public string SuperFanRankName()
        {
            switch(SuperFanRank)
            {
                case 0:
                    return "";
                case 1:
                    return "1K";
                case 2:
                    return "5K";
                case 3:
                    return "10K";
                case 4:
                    return "30K";
                case 5:
                    return "50K";
                case 6:
                    return "100K";
                case 7:
                    return "200K";
                case 8:
                    return "300K";
                case 9:
                    return "500K";
            }
            return "不明";
        }
    }
}
