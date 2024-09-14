using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Exceptions;
using Substrate.NET.Metadata.Service;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V9;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.Tests
{
    public class MetadataServiceBasicTest : MetadataBaseTest
    {
        private IList<PalletCallMetadataV9> generatePalletCallMetadataV9(int nb)
        {
            return Enumerable.Range(1, nb).Select(x => new PalletCallMetadataV9()
            {
                Name = new Str($"Test_{x}"),
                Docs = new BaseVec<Str>(new Str[] { new Str($"DocumentationCall_{x}") }),
                Args = new BaseVec<PalletCallArgsMetadataV9>(new PalletCallArgsMetadataV9[] {
                        new PalletCallArgsMetadataV9()
                        {
                            Name = new Str($"PalletCallArgsMetadataV9_Name_{x}"),
                            CallType = new Str($"PalletCallArgsMetadataV9_CallType_{x}")
                        }
                    })
            }).ToList();
        }

        private IList<PalletEventMetadataV9> generatePalletEventMetadataV9(int nb)
        {
            return Enumerable.Range(1, nb).Select(x => new PalletEventMetadataV9()
            {
                Name = new Str($"TestEvent_{x}"),
                Docs = new BaseVec<Str>(new Str[] { new Str($"DocumentationEvent_{x}") }),
                Args = new BaseVec<Str>(new Str[] { new Str($"ArgsEvent_{x}") })
            }).ToList();
        }

        private IList<PalletConstantMetadataV9> generatePalletConstantsMetadataV9(int nb)
        {
            var u8 = new U8();
            u8.Create(byte.MaxValue);
            var value = new ByteGetter();
            value.Create(new U8[] { u8 });

            return Enumerable.Range(1, nb).Select(x => new PalletConstantMetadataV9()
            {
                Name = new Str($"TestConstant_{x}"),
                ConstantType = new Str("ConstantType"),
                Documentation = new BaseVec<Str>(new Str[] { new Str($"DocumentationConstant_{x}") }),
                Value = value
            }).ToList();
        }

        private IList<PalletErrorMetadataV9> generatePalletErrorsMetadataV9(int nb)
        {
            return Enumerable.Range(1, nb).Select(x => new PalletErrorMetadataV9()
            {
                Name = new Str($"TestError_{x}"),
                Docs = new BaseVec<Str>(new Str[] { new Str($"DocumentationError_{x}") }),
            }).ToList();
        }

        [Test]
        public void MetadataV9_PalletCallDiff_ShouldSuceed()
        {
            IList<PalletCallMetadataV9> calls1 = generatePalletCallMetadataV9(4);
            IList<PalletCallMetadataV9> calls2 = generatePalletCallMetadataV9(4);

            var calls_1 = new List<PalletCallMetadataV9>() { calls1[0], calls1[1], calls1[2] };

            calls2[2].Args.Value[0].CallType.Value = "Changed !";
            var calls_2 = new List<PalletCallMetadataV9>() { calls2[1], calls2[2], calls2[3] };

            var res = MetadataUtils.CompareName(calls_1, calls_2).ToList();

            Assert.That(res, Is.Not.Null);
            Assert.That(res[0], Is.EqualTo((CompareStatus.Added, calls2[3])));
            Assert.That(res[1], Is.EqualTo((CompareStatus.Removed, calls1[0])));
        }

        [Test]
        public void MetadataV9_PalletEventDiff_ShouldSuceed()
        {
            IList<PalletEventMetadataV9> calls1 = generatePalletEventMetadataV9(4);
            IList<PalletEventMetadataV9> calls2 = generatePalletEventMetadataV9(4);

            var calls_1 = new List<PalletEventMetadataV9>() { calls1[0], calls1[1], calls1[2] };

            calls2[2].Args.Value[0].Value = "Changed !";
            var calls_2 = new List<PalletEventMetadataV9>() { calls2[1], calls2[2], calls2[3] };

            var res = MetadataUtils.CompareName(calls_1, calls_2).ToList();

            Assert.That(res, Is.Not.Null);
            Assert.That(res[0], Is.EqualTo((CompareStatus.Added, calls2[3])));
            Assert.That(res[1], Is.EqualTo((CompareStatus.Removed, calls1[0])));
        }

        [Test]
        public void MetadataV9_PalletConstantDiff_ShouldSuceed()
        {
            IList<PalletConstantMetadataV9> calls1 = generatePalletConstantsMetadataV9(4);
            IList<PalletConstantMetadataV9> calls2 = generatePalletConstantsMetadataV9(4);

            var calls_1 = new List<PalletConstantMetadataV9>() { calls1[0], calls1[1], calls1[2] };

            calls2[2].ConstantType.Value = "Changed !";
            var calls_2 = new List<PalletConstantMetadataV9>() { calls2[1], calls2[2], calls2[3] };

            var res = MetadataUtils.CompareName(calls_1, calls_2).ToList();

            Assert.That(res, Is.Not.Null);
            Assert.That(res[0], Is.EqualTo((CompareStatus.Added, calls2[3])));
            Assert.That(res[1], Is.EqualTo((CompareStatus.Removed, calls1[0])));
        }

        [Test]
        public void MetadataV9_PalletErrorDiff_ShouldSuceed()
        {
            IList<PalletErrorMetadataV9> calls1 = generatePalletErrorsMetadataV9(4);
            IList<PalletErrorMetadataV9> calls2 = generatePalletErrorsMetadataV9(4);

            var calls_1 = new List<PalletErrorMetadataV9>() { calls1[0], calls1[1], calls1[2] };

            calls2[2].Docs.Value[0].Value = "Changed !";
            var calls_2 = new List<PalletErrorMetadataV9>() { calls2[1], calls2[2], calls2[3] };

            var res = MetadataUtils.CompareName(calls_1, calls_2).ToList();

            Assert.That(res, Is.Not.Null);
            Assert.That(res[0], Is.EqualTo((CompareStatus.Added, calls2[3])));
            Assert.That(res[1], Is.EqualTo((CompareStatus.Removed, calls1[0])));
        }

        [Test]
        public void Metadata_CheckDifferentialPallet_ShouldSuceed()
        {
            var m1 = new List<ModuleMetadataV11>()
            {
                new ModuleMetadataV11()
                {
                    Name = new Str("Module_1")
                },
                new ModuleMetadataV11()
                {
                    Name = new Str("Module_2")
                },
                new ModuleMetadataV11()
                {
                    Name = new Str("Module_3")
                }
            };

            var m2 = new List<ModuleMetadataV11>()
            {
                new ModuleMetadataV11()
                {
                    Name = new Str("Module_2")
                },
                new ModuleMetadataV11()
                {
                    Name = new Str("Module_3")
                },
                new ModuleMetadataV11()
                {
                    Name = new Str("Module_4")
                }
            };

            Assert.That(MetadataUtils.CompareName<ModuleMetadataV11>(null, null).Count(), Is.EqualTo(0));

            var resOnlyFirstListNull = MetadataUtils.CompareName(m1, null);
            Assert.That(resOnlyFirstListNull.All(x => x.Item1 == CompareStatus.Removed), Is.True);
            Assert.That(resOnlyFirstListNull.Count(), Is.EqualTo(m1.Count));

            var resOnlyFirstListEmpty = MetadataUtils.CompareName(m1, new List<ModuleMetadataV11>());
            Assert.That(resOnlyFirstListEmpty.All(x => x.Item1 == CompareStatus.Removed), Is.True);
            Assert.That(resOnlyFirstListEmpty.Count(), Is.EqualTo(m1.Count));

            var resOnlySecondListNull = MetadataUtils.CompareName(null, m2);
            Assert.That(resOnlySecondListNull.All(x => x.Item1 == CompareStatus.Added), Is.True);
            Assert.That(resOnlySecondListNull.Count(), Is.EqualTo(m2.Count));

            var resOnlySecondListEmpty = MetadataUtils.CompareName(null, m2);
            Assert.That(resOnlySecondListEmpty.All(x => x.Item1 == CompareStatus.Added), Is.True);
            Assert.That(resOnlySecondListEmpty.Count(), Is.EqualTo(m2.Count));

            var res = MetadataUtils.CompareName(m1, m2).ToList();
            Assert.That(res[0].Item1, Is.EqualTo(CompareStatus.Added));
            Assert.That(res[1].Item1, Is.EqualTo(CompareStatus.Removed));

        }

        [Test]
        public void DifferentMetadataMajorVersion_ShouldFail()
        {
            Assert.Throws<MetadataException>(() => MetadataUtils.EnsureMetadataVersion(
                readMetadataFromFile("V11\\MetadataV11_0"), 
                readMetadataFromFile("V14\\MetadataV14_9420")));
        }

        [Test]
        public void MetadataV14_GetMetadataVersion_ShouldSucceed()
        {
            Assert.That(
                MetadataUtils.GetMetadataVersion(readMetadataFromFile("V14\\MetadataV14_9420")), 
                Is.EqualTo(MetadataVersion.V14));
        }
    }
}
