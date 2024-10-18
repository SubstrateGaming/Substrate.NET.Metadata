using Newtonsoft.Json.Linq;
using Substrate.NetApi;
using Substrate.NetApi.Model.Extrinsics;
using Substrate.NetApi.Model.Types.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Substrate.NET.Metadata.Node.Tests.Storage
{
    internal class AccountInfoTests
    {


        private SubstrateClient _substrateClient;

        [OneTimeSetUp]
        public async Task SetupAsync()
        {
            //_substrateClient = new SubstrateClient(new Uri("wss://polkadot-rpc.dwellir.com"), ChargeTransactionPayment.Default());
            _substrateClient = new SubstrateClient(new Uri("wss://polkadot.api.onfinality.io/public-ws"), ChargeTransactionPayment.Default());

            await _substrateClient.ConnectAsync();
            Assert.That(_substrateClient.IsConnected, Is.True);
        }

        [OneTimeTearDown]
        public void Teardown()
        {
            _substrateClient.Dispose();
        }

        /// <summary>
        /// Blockchain : Polkadot
        /// Fetch Account Info from block 1
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task AccountInfo_MetadataV11_ShouldBeCorrectlyInstanciatedAsync()
        {
            var accountId32 = new AccountId32();
            accountId32.Create(Utils.GetPublicKeyFrom("12H7nsDUrJUSCQQJrTKAFfyCWSactiSdjoVUixqcd9CZHTGt"));

            var parameters = RequestGenerator.GetStorage("System", "Account", Substrate.NetApi.Model.Meta.Storage.Type.Map, [Substrate.NetApi.Model.Meta.Storage.Hasher.BlakeTwo128Concat], [accountId32]);

            var result = await _substrateClient.GetStorageAsync<AccountInfoOldVersion>(parameters, "0xC0096358534EC8D21D01D34B836EED476A1C343F8724FA2153DC0725AD797A90", CancellationToken.None);

            Assert.That(result.Data, Is.Not.Null);
        }
    }

    public sealed class AccountInfoOldVersion : BaseType
    {
        // https://docs.rs/frame-system/2.0.0/frame_system/struct.AccountInfo.html
        public Substrate.NetApi.Model.Types.Primitive.U32 Nonce { get; set; } = default!;
        public Substrate.NetApi.Model.Types.Primitive.U8 RefCount { get; set; } = default!;
        public AccountDataOldVersion Data { get; set; } = default!;

        public override System.String TypeName()
        {
            return "AccountInfo";
        }

        public override System.Byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Nonce.Encode());
            //result.AddRange(Consumers.Encode());
            //result.AddRange(Providers.Encode());
            //result.AddRange(Sufficients.Encode());
            result.AddRange(RefCount.Encode());
            result.AddRange(Data.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Nonce = new Substrate.NetApi.Model.Types.Primitive.U32();
            Nonce.Decode(byteArray, ref p);
            RefCount = new Substrate.NetApi.Model.Types.Primitive.U8();
            RefCount.Decode(byteArray, ref p);
            Data = new AccountDataOldVersion();
            Data.Decode(byteArray, ref p);
            var bytesLength = p - start;
            TypeSize = bytesLength;
            Bytes = new byte[bytesLength];
            System.Array.Copy(byteArray, start, Bytes, 0, bytesLength);
        }
    }

    public sealed class AccountDataOldVersion : BaseType
    {
        // https://docs.rs/pallet-balances/2.0.1/pallet_balances/struct.Instance0.html

        public Substrate.NetApi.Model.Types.Primitive.U128 Free { get; set; } = default!;
        public Substrate.NetApi.Model.Types.Primitive.U128 Reserved { get; set; } = default!;
        public Substrate.NetApi.Model.Types.Primitive.U128 MiscFrozen { get; set; } = default!;
        public Substrate.NetApi.Model.Types.Primitive.U128 FeeFrozen { get; set; } = default!;

        public override System.String TypeName()
        {
            return "AccountData";
        }

        public override System.Byte[] Encode()
        {
            var result = new List<byte>();
            result.AddRange(Free.Encode());
            result.AddRange(Reserved.Encode());
            result.AddRange(MiscFrozen.Encode());
            result.AddRange(FeeFrozen.Encode());
            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            Free = new Substrate.NetApi.Model.Types.Primitive.U128();
            Free.Decode(byteArray, ref p);
            Reserved = new Substrate.NetApi.Model.Types.Primitive.U128();
            Reserved.Decode(byteArray, ref p);
            MiscFrozen = new Substrate.NetApi.Model.Types.Primitive.U128();
            MiscFrozen.Decode(byteArray, ref p);
            FeeFrozen = new Substrate.NetApi.Model.Types.Primitive.U128();
            FeeFrozen.Decode(byteArray, ref p);
            var bytesLength = p - start;
            TypeSize = bytesLength;
            Bytes = new byte[bytesLength];
            System.Array.Copy(byteArray, start, Bytes, 0, bytesLength);
        }
    }

    public sealed class AccountId32 : BaseType
    {
        public Substrate.NetApi.Model.Types.Primitive.U8[] Value { get; set; } = default!;

        public override System.String TypeName()
        {
            return "AccountId32";
        }

        public override int TypeSize
        {
            get
            {
                return 32;
            }
        }

        public override System.Byte[] Encode()
        {
            var result = new List<byte>();
            foreach (var v in Value)
            {
                result.AddRange(v.Encode());
            }

            return result.ToArray();
        }

        public override void Decode(byte[] byteArray, ref int p)
        {
            var start = p;
            var array = new Substrate.NetApi.Model.Types.Primitive.U8[TypeSize];
            for (var i = 0; i < array.Length; i++)
            {
                var t = new Substrate.NetApi.Model.Types.Primitive.U8();
                t.Decode(byteArray, ref p);
                array[i] = t;
            }

            var bytesLength = p - start;
            Bytes = new byte[bytesLength];
            System.Array.Copy(byteArray, start, Bytes, 0, bytesLength);
            Value = array;
        }

        public void Create(Substrate.NetApi.Model.Types.Primitive.U8[] array)
        {
            Value = array;
            Bytes = Encode();
        }
    }
}
