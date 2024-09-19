using Substrate.NET.Metadata.Conversion.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Substrate.NET.Metadata.Conversion
{
    internal static class ManualNodes
    {
        public static CustomNodeBuilder All(uint? specVersion)
        {
            var customNodeBuilder = new CustomNodeBuilder();

            var accountInfos = AccountInfo();
            if (specVersion is null)
            {
                customNodeBuilder.Add(accountInfos.First());
            } else
            {
                var optionalCustomVersion = accountInfos.SingleOrDefault(x => x.IsVersionValid(specVersion!.Value));
                if (optionalCustomVersion is not null)
                {
                    customNodeBuilder.Add(optionalCustomVersion);
                }
            }

            return customNodeBuilder;
        }

        /// <summary>
        /// The account info old metadata (https://docs.rs/frame-system/15.0.0/frame_system/struct.AccountInfo.html)
        /// </summary>
        /// <param name="customNodeBuilder"></param>
        /// <returns></returns>
        public static List<CustomCompositeBuilder> AccountInfo()
        {
            return new List<CustomCompositeBuilder>()
            {
                new CustomCompositeBuilder()
                .FromVersion(0).ToVersion(10)
                .AddField("nonce", "Index")
                .AddField("refcount", "u8")
                .AddField("data", "AccountData")
                .WithPath("frame_system", "AccountInfo"),

                new CustomCompositeBuilder()
                .FromVersion(11).ToVersion(20)
                .AddField("nonce", "Index")
                .AddField("refcount", "u32")
                .AddField("data", "AccountData")
                .WithPath("frame_system", "AccountInfo"),

                new CustomCompositeBuilder()
                .FromVersion(21).ToVersion(30)
                .AddField("nonce", "Index")
                .AddField("consumers", "u32")
                .AddField("providers", "u32")
                .AddField("data", "AccountData")
                .WithPath("frame_system", "AccountInfo"),

                new CustomCompositeBuilder()
                .FromVersion(31).ToVersion(40)
                .AddField("nonce", "Index")
                .AddField("consumers", "u32")
                .AddField("providers", "u32")
                .AddField("sufficients", "u32")
                .AddField("data", "AccountData")
                .WithPath("frame_system", "AccountInfo")
            };
        }
    }
}
