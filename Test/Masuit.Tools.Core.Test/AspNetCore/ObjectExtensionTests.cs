using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Masuit.Tools.Systems;
using Xunit;

namespace Masuit.Tools.Core.Test.AspNetCore
{
    public class ObjectExtensionTests : TestBase
    {
        [Fact]
        public void ToJsonTest()
        {
            var obj = new
            {
                Id = 1,
                name = "van",
                addressInfo = new
                {
                    street = "123 Main St",
                    city = "zcvz",
                },
                remarks = new List<string> { "Deep  Dark  Fantastic", "爱玩游戏♂" },
            };

            var json = obj.ToJson();

            Assert.Equal(
                "{\"Id\":1,\"name\":\"van\",\"addressInfo\":{\"street\":\"123 Main St\",\"city\":\"zcvz\"},\"remarks\":[\"Deep  Dark  Fantastic\",\"爱玩游戏\u2642\"]}",
                json);
        }

        [Fact]
        public void ToJsonIgnoreNullTest()
        {
            var obj = new People
            {
                Name = "van",
                Age = 52,
            };

            var json = obj.ToJsonIgnoreNull();

            Assert.Equal(
                "{\"Name\":\"van\",\"Age\":52}",
                json);
        }

        [Fact]
        public void FromObjectTest()
        {
            var json = "{\"Name\":\"van\",\"Age\":52}";

            var obj = json.ToObject<People>();

            Assert.Equal(
                "van",
                obj.Name);
        }


        [Fact]
        public void SerializeSortByNameTest()
        {
            var obj = new People
            {
                Name = "van",
                Age = 52,
            };

            var json = obj.ToJson(new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                TypeInfoResolver = new SerializeSortByNameResolver()
            });

            Assert.Equal(
                "{\"Age\":52,\"Name\":\"van\"}",
                json);
        }


        private class People
        {
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}