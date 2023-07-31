using Ardalis.GuardClauses;
using Substrate.NET.Metadata.Base;
using Substrate.NET.Metadata.Base.Portable;
using Substrate.NET.Metadata.Compare;
using Substrate.NET.Metadata.Compare.TypeDef;
using Substrate.NET.Metadata.Exceptions;
using Substrate.NET.Metadata.V10;
using Substrate.NET.Metadata.V11;
using Substrate.NET.Metadata.V12;
using Substrate.NET.Metadata.V13;
using Substrate.NET.Metadata.V14;
using Substrate.NET.Metadata.V9;
using Substrate.NetApi.Model.Types;
using Substrate.NetApi.Model.Types.Base;
using Substrate.NetApi.Model.Types.Primitive;

namespace Substrate.NET.Metadata.Service
{
    public class MetadataService : IMetadataService
    {
        /// <summary>
        /// Get major version from metadata
        /// </summary>
        /// <param name="hexMetadata"></param>
        /// <returns>The major version (V9 to v15)</returns>
        /// <exception cref="MetadataException"></exception>
        public MetadataVersion GetMetadataVersion(string hexMetadata)
        {
            Guard.Against.NullOrEmpty(hexMetadata);
            CheckRuntimeMetadata checkVersion = new(hexMetadata);

            return checkVersion.MetaDataInfo.Version.Value switch
            {
                9 => MetadataVersion.V9,
                10 => MetadataVersion.V10,
                11 => MetadataVersion.V11,
                12 => MetadataVersion.V12,
                13 => MetadataVersion.V13,
                14 => MetadataVersion.V14,
                15 => MetadataVersion.V15,
                _ => throw new MetadataException($"Metadata version {checkVersion.MetaDataInfo.Version.Value} is not supported")
            };
        }

        /// <summary>
        /// Check if metadatas have same major version
        /// </summary>
        /// <param name="hexMetadata1"></param>
        /// <param name="hexMetadata2"></param>
        /// <returns></returns>
        /// <exception cref="MetadataException"></exception>
        public MetadataVersion EnsureMetadataVersion(string hexMetadata1, string hexMetadata2)
        {
            // To be compared, Metadata should have same Major version
            var v1 = GetMetadataVersion(hexMetadata1);
            var v2 = GetMetadataVersion(hexMetadata2);

            if (v1 != v2)
                throw new MetadataException($"Cannot compare metadata v{v1} and v{v2}. Major version have to be the same.");

            return v1;
        }

        #region Metadata compare
        /// <summary>
        /// Compare element by name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public IEnumerable<(CompareStatus, T)> CompareName<T>(
            IEnumerable<T>? source,
            IEnumerable<T>? destination)
            where T : IMetadataName
            => MetadataModuleCompare(source, destination, (x, y) => x.Name.Value == y.Name.Value);

        /// <summary>
        /// Compare element by type
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public IEnumerable<(CompareStatus, T)> CompareType<T>(
            IEnumerable<T>? source,
            IEnumerable<T>? destination)
            where T : IMetadataType
            => MetadataModuleCompare(source, destination, (x, y) => x.ElemType.Value == y.ElemType.Value);

        /// <summary>
        /// Compare element by type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public IEnumerable<(CompareStatus, TType)> CompareType(
            IEnumerable<TType>? source,
            IEnumerable<TType>? destination)
            => MetadataModuleCompare(source, destination, (x, y) => x.Value == y.Value);

        /// <summary>
        /// Remove non shared modules from given module list
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="modulesStart"></param>
        /// <param name="uncommonModules"></param>
        /// <param name="status"></param>
        /// <returns></returns>
        protected IEnumerable<T> FilterModuleByStatus<T>(IEnumerable<T> modulesStart, IEnumerable<(CompareStatus, T)> uncommonModules, CompareStatus status)
            where T : IMetadataName, IType, new()
        {
            if (uncommonModules.Any())
            {
                var modulesToRemove = uncommonModules.Where(x => x.Item1 == status);
                modulesStart = modulesStart.Where(x => !modulesToRemove.Any(y => y.Item2.Name.Value == x.Name.Value));
            }

            return modulesStart.OrderBy(x => x.Name.Value);
        }

        #endregion

        #region Utils
        /// <summary>
        /// Compare module metadata from version 9 to 13
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private IEnumerable<(CompareStatus, T)> MetadataModuleCompare<T>(
            IEnumerable<T>? source,
            IEnumerable<T>? destination,
            Func<T, T, bool> predicate)
        {
            var res = new List<(CompareStatus, T)>();

            if (source == null && destination == null) return res;
            if (source == null && destination != null) return destination.Select(x => (CompareStatus.Added, x));
            if (source != null && destination == null) return source.Select(x => (CompareStatus.Removed, x));

            // First we check what call have been added
            var added = destination!.Where(x => !source!.Any(y => predicate(x, y)));
            res.AddRange(added.Select(x => (CompareStatus.Added, x)));

            var removed = source!.Where(x => !destination!.Any(y => predicate(x, y)));
            res.AddRange(removed.Select(x => (CompareStatus.Removed, x)));

            return res;
        }

        /// <summary>
        /// Compare list of Substrate string type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool AreStringsEquals(BaseVec<Str> source, BaseVec<Str> target)
            => AreStringsEquals(source.Value, target.Value);

        /// <summary>
        /// Compare list of Substrate string type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool AreStringsEquals(IEnumerable<Str> source, IEnumerable<Str> target)
        {
            return !source.Select(x => x.Value).Except(target.Select(x => x.Value)).Any();
        }

        private IEnumerable<T> Sanitize<T>(IEnumerable<BaseVec<T>> elems)
            where T : IType, new()
        {
            if (elems == null) return Enumerable.Empty<T>();

            var res = elems
                .SelectMany(x => x.Value);

            return res;
        }

        private IEnumerable<T> Sanitize<T>(BaseOpt<BaseVec<T>> elems)
            where T : IType, new()
        {
            if (elems == null || !elems.OptionFlag) return Enumerable.Empty<T>();

            return elems.Value.Value;
        }

        private IEnumerable<T> Sanitize<T>(BaseVec<T> elems)
            where T : IType, new()
        {
            if (elems == null) return Enumerable.Empty<T>();

            return elems.Value;
        }
        #endregion

        #region Compare V9
        public MetadataDiffV9 MetadataCompareV9(MetadataV9 m1, MetadataV9 m2)
        {
            var resModulesDiff = new List<MetadataDifferentialModulesV9>();

            // Check added / removed pallet
            var nonCommonModules = CompareName(Sanitize(m1.RuntimeMetadataData.Modules), Sanitize(m2.RuntimeMetadataData.Modules));

            resModulesDiff.AddRange(nonCommonModules.Select(x => x.Item2.ToDifferentialModules(x.Item1)));

            // We insert added modules
            var m1CommonModules = FilterModuleByStatus(m1.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Removed);
            var m2CommonModules = FilterModuleByStatus(m2.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Added);

            foreach (var elem in m1CommonModules.Zip(m2CommonModules))
            {
                resModulesDiff.Add(new MetadataDifferentialModulesV9()
                {
                    ModuleName = elem.First.Name.Value,
                    Storage = CompareModuleStorageV9(elem.First.Storage.Value, elem.Second.Storage.Value),
                    Calls = CompareName(Sanitize(elem.First.Calls), Sanitize(elem.Second.Calls)),
                    Events = CompareName(Sanitize(elem.First.Events), Sanitize(elem.Second.Events)),
                    Constants = CompareName(Sanitize(elem.First.Constants), Sanitize(elem.Second.Constants)),
                    Errors = CompareName(Sanitize(elem.First.Errors), Sanitize(elem.Second.Errors))
                });
            }

            return new MetadataDiffV9()
            {
                AllModulesDiff = resModulesDiff
            };
        }

        protected IEnumerable<(string prefix, (CompareStatus status, StorageEntryMetadataV9 storage))> CompareModuleStorageV9(
            PalletStorageMetadataV9 source,
            PalletStorageMetadataV9 destination)
        {
            if (source == null && destination == null)
                return Enumerable.Empty<(string prefix, (CompareStatus status, StorageEntryMetadataV9 storage))>();

            string prefix = source != null ? source.Prefix.Value : destination!.Prefix.Value;

            return CompareName(
                source != null ? Sanitize(source.Entries) : null,
                destination != null ? Sanitize(destination.Entries) : null)
                .Select(x => (prefix, x));
        }

        #endregion

        #region Compare V10
        public MetadataDiffV10 MetadataCompareV10(MetadataV10 m1, MetadataV10 m2)
        {
            var resModulesDiff = new List<MetadataDifferentialModulesV10>();

            // Check added / removed pallet
            var nonCommonModules = CompareName(Sanitize(m1.RuntimeMetadataData.Modules), Sanitize(m2.RuntimeMetadataData.Modules));

            resModulesDiff.AddRange(nonCommonModules.Select(x => x.Item2.ToDifferentialModules(x.Item1)));

            // We insert added modules
            var m1CommonModules = FilterModuleByStatus(m1.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Removed);
            var m2CommonModules = FilterModuleByStatus(m2.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Added);

            if (m1CommonModules.Count() != m2CommonModules.Count()) throw new InvalidOperationException("Metadata modules should be equals !");

            foreach (var elem in m1CommonModules.Zip(m2CommonModules))
            {
                resModulesDiff.Add(new MetadataDifferentialModulesV10()
                {
                    ModuleName = elem.First.Name.Value,
                    Storage = CompareModuleStorageV10(elem.First.Storage.Value, elem.Second.Storage.Value),
                    Calls = CompareName(Sanitize(elem.First.Calls), Sanitize(elem.Second.Calls)),
                    Events = CompareName(Sanitize(elem.First.Events), Sanitize(elem.Second.Events)),
                    Constants = CompareName(Sanitize(elem.First.Constants), Sanitize(elem.Second.Constants)),
                    Errors = CompareName(Sanitize(elem.First.Errors), Sanitize(elem.Second.Errors))
                });
            }

            return new MetadataDiffV10()
            {
                AllModulesDiff = resModulesDiff
            };
        }

        protected IEnumerable<(string prefix, (CompareStatus status, StorageEntryMetadataV10 storage))> CompareModuleStorageV10(
            PalletStorageMetadataV10? source,
            PalletStorageMetadataV10? destination)
        {
            if (source == null && destination == null)
                return Enumerable.Empty<(string prefix, (CompareStatus status, StorageEntryMetadataV10 storage))>();

            string prefix = source != null ? source.Prefix.Value : destination!.Prefix.Value;

            return CompareName(
                source != null ? Sanitize(source.Entries) : null,
                destination != null ? Sanitize(destination.Entries) : null)
                .Select(x => (prefix, x));
        }
        #endregion

        #region Compare V11
        public MetadataDiffV11 MetadataCompareV11(MetadataV11 m1, MetadataV11 m2)
        {
            var resModulesDiff = new List<MetadataDifferentialModulesV11>();

            // Check added / removed pallet
            var nonCommonModules = CompareName(Sanitize(m1.RuntimeMetadataData.Modules), Sanitize(m2.RuntimeMetadataData.Modules));

            resModulesDiff.AddRange(nonCommonModules.Select(x => x.Item2.ToDifferentialModule(x.Item1)));

            // We insert added modules
            var m1CommonModules = FilterModuleByStatus(m1.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Removed);
            var m2CommonModules = FilterModuleByStatus(m2.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Added);

            if (m1CommonModules.Count() != m2CommonModules.Count()) throw new InvalidOperationException("Metadata modules should be equals !");

            foreach (var elem in m1CommonModules.Zip(m2CommonModules))
            {
                resModulesDiff.Add(new MetadataDifferentialModulesV11()
                {
                    ModuleName = elem.First.Name.Value,
                    Storage = CompareModuleStorageV11(elem.First.Storage.Value, elem.Second.Storage.Value),
                    Calls = CompareName(Sanitize(elem.First.Calls), Sanitize(elem.Second.Calls)),
                    Events = CompareName(Sanitize(elem.First.Events), Sanitize(elem.Second.Events)),
                    Constants = CompareName(Sanitize(elem.First.Constants), Sanitize(elem.Second.Constants)),
                    Errors = CompareName(Sanitize(elem.First.Errors), Sanitize(elem.Second.Errors))
                });
            }

            return new MetadataDiffV11()
            {
                AllModulesDiff = resModulesDiff
            };
        }

        protected IEnumerable<(string prefix, (CompareStatus status, StorageEntryMetadataV11 storage))> CompareModuleStorageV11(
            PalletStorageMetadataV11? source,
            PalletStorageMetadataV11? destination)
        {
            if (source == null && destination == null)
                return Enumerable.Empty<(string prefix, (CompareStatus status, StorageEntryMetadataV11 storage))>();

            string prefix = source != null ? source.Prefix.Value : destination!.Prefix.Value;

            return CompareName(
                source != null ? Sanitize(source.Entries) : null,
                destination != null ? Sanitize(destination.Entries) : null)
                .Select(x => (prefix, x));
        }
        #endregion

        #region Compare V12
        public MetadataDiffV12 MetadataCompareV12(MetadataV12 m1, MetadataV12 m2)
        {
            var resModulesDiff = new List<MetadataDifferentialModulesV12>();

            // Check added / removed pallet
            var nonCommonModules = CompareName(Sanitize(m1.RuntimeMetadataData.Modules), Sanitize(m2.RuntimeMetadataData.Modules));

            resModulesDiff.AddRange(nonCommonModules.Select(x => x.Item2.ToDifferentialModules(x.Item1)));

            // We insert added modules
            var m1CommonModules = FilterModuleByStatus(m1.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Removed);
            var m2CommonModules = FilterModuleByStatus(m2.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Added);

            if (m1CommonModules.Count() != m2CommonModules.Count()) throw new InvalidOperationException("Metadata modules should be equals !");

            foreach (var elem in m1CommonModules.Zip(m2CommonModules))
            {
                resModulesDiff.Add(new MetadataDifferentialModulesV12()
                {
                    ModuleName = elem.First.Name.Value,
                    Storage = CompareModuleStorageV12(elem.First.Storage.Value, elem.Second.Storage.Value),
                    Calls = CompareName(Sanitize(elem.First.Calls), Sanitize(elem.Second.Calls)),
                    Events = CompareName(Sanitize(elem.First.Events), Sanitize(elem.Second.Events)),
                    Constants = CompareName(Sanitize(elem.First.Constants), Sanitize(elem.Second.Constants)),
                    Errors = CompareName(Sanitize(elem.First.Errors), Sanitize(elem.Second.Errors))
                });
            }

            return new MetadataDiffV12()
            {
                AllModulesDiff = resModulesDiff
            };
        }

        protected IEnumerable<(string prefix, (CompareStatus status, StorageEntryMetadataV11 storage))> CompareModuleStorageV12(
            PalletStorageMetadataV12? source,
            PalletStorageMetadataV12? destination)
        {
            if (source == null && destination == null)
                return Enumerable.Empty<(string prefix, (CompareStatus status, StorageEntryMetadataV11 storage))>();

            string prefix = source != null ? source.Prefix.Value : destination!.Prefix.Value;

            return CompareName(
                source != null ? Sanitize(source.Entries) : null,
                destination != null ? Sanitize(destination.Entries) : null)
                .Select(x => (prefix, x));
        }
        #endregion

        #region Compare V13
        public MetadataDiffV13 MetadataCompareV13(MetadataV13 m1, MetadataV13 m2)
        {
            var resModulesDiff = new List<MetadataDifferentialModulesV13>();

            // Check added / removed pallet
            var nonCommonModules = CompareName(Sanitize(m1.RuntimeMetadataData.Modules), Sanitize(m2.RuntimeMetadataData.Modules));

            resModulesDiff.AddRange(nonCommonModules.Select(x => x.Item2.ToDifferentialModules(x.Item1)));

            // We insert added modules
            var m1CommonModules = FilterModuleByStatus(m1.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Removed);
            var m2CommonModules = FilterModuleByStatus(m2.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Added);

            if (m1CommonModules.Count() != m2CommonModules.Count()) throw new InvalidOperationException("Metadata modules should be equals !");

            foreach (var elem in m1CommonModules.Zip(m2CommonModules))
            {
                resModulesDiff.Add(new MetadataDifferentialModulesV13()
                {
                    ModuleName = elem.First.Name.Value,
                    Storage = CompareModuleStorageV13(elem.First.Storage.Value, elem.Second.Storage.Value),
                    Calls = CompareName(Sanitize(elem.First.Calls), Sanitize(elem.Second.Calls)),
                    Events = CompareName(Sanitize(elem.First.Events), Sanitize(elem.Second.Events)),
                    Constants = CompareName(Sanitize(elem.First.Constants), Sanitize(elem.Second.Constants)),
                    Errors = CompareName(Sanitize(elem.First.Errors), Sanitize(elem.Second.Errors))
                });
            }

            return new MetadataDiffV13()
            {
                AllModulesDiff = resModulesDiff
            };
        }

        protected IEnumerable<(string prefix, (CompareStatus status, StorageEntryMetadataV13 storage))> CompareModuleStorageV13(
            PalletStorageMetadataV13? source,
            PalletStorageMetadataV13? destination)
        {
            if (source == null && destination == null)
                return Enumerable.Empty<(string prefix, (CompareStatus status, StorageEntryMetadataV13 storage))>();

            string prefix = source != null ? source.Prefix.Value : destination!.Prefix.Value;

            return CompareName(
                source != null ? Sanitize(source.Entries) : null,
                destination != null ? Sanitize(destination.Entries) : null)
                .Select(x => (prefix, x));
        }
        #endregion

        #region Compare V14
        public MetadataDiffV14 MetadataCompareV14(MetadataV14 m1, MetadataV14 m2)
        {
            var resModulesDiff = new List<MetadataDifferentialModulesV14>();

            // Check added / removed pallet
            var nonCommonModules = CompareName(Sanitize(m1.RuntimeMetadataData.Modules), Sanitize(m2.RuntimeMetadataData.Modules));


            resModulesDiff.AddRange(nonCommonModules.Select(x =>
            {
                if (x.Item1 == CompareStatus.Added)
                    return x.Item2.ToDifferentialModules(x.Item1, m2.RuntimeMetadataData.Lookup);
                else
                    return x.Item2.ToDifferentialModules(x.Item1, m1.RuntimeMetadataData.Lookup);
            }));

            // We insert added modules
            var m1CommonModules = FilterModuleByStatus(m1.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Removed);
            var m2CommonModules = FilterModuleByStatus(m2.RuntimeMetadataData.Modules.Value, nonCommonModules, CompareStatus.Added);

            if (m1CommonModules.Count() != m2CommonModules.Count()) throw new InvalidOperationException("Metadata modules should be equals !");

            foreach (var elem in m1CommonModules.Zip(m2CommonModules))
            {
                resModulesDiff.Add(new MetadataDifferentialModulesV14()
                {
                    ModuleName = elem.First.Name.Value,
                    Storage = CompareModuleStorageV14(elem.First.Storage.Value, elem.Second.Storage.Value),
                    Calls = CompareModuleV14(elem.First.Calls, elem.Second.Calls, m1.RuntimeMetadataData.Lookup, m2.RuntimeMetadataData.Lookup),
                    Events = CompareModuleV14(elem.First.Events, elem.Second.Events, m1.RuntimeMetadataData.Lookup, m2.RuntimeMetadataData.Lookup),
                    Constants = CompareName(Sanitize(elem.First.Constants), Sanitize(elem.Second.Constants)),
                    Errors = CompareModuleV14(elem.First.Errors, elem.Second.Errors, m1.RuntimeMetadataData.Lookup, m2.RuntimeMetadataData.Lookup)
                });
            }

            return new MetadataDiffV14()
            {
                AllModulesDiff = resModulesDiff
            };
        }

        protected LookupDifferential CompareModuleV14<T>(
            BaseOpt<T> source,
            BaseOpt<T> destination,
            PortableRegistry lookupSource,
            PortableRegistry lookupDestination)
            where T : BaseType, IMetadataType, new()
        {
            if (!source.OptionFlag && !destination.OptionFlag)
                return new LookupDifferential();

            if (source.OptionFlag && !destination.OptionFlag)
                return LookupDifferential.FromLookup(CompareStatus.Removed, LookupDifferential.FindType(lookupSource, source.Value.ElemType));

            if (!source.OptionFlag && destination.OptionFlag)
                return LookupDifferential.FromLookup(CompareStatus.Added, LookupDifferential.FindType(lookupDestination, destination.Value.ElemType));

            return CompareLookup(
                (uint)source.Value.ElemType.Value.Value,
                (uint)destination.Value.ElemType.Value.Value, lookupSource, lookupDestination);
        }

        protected IEnumerable<(string prefix, (CompareStatus status, StorageEntryMetadataV14 storage))> CompareModuleStorageV14(
            PalletStorageMetadataV14? source,
            PalletStorageMetadataV14? destination)
        {
            if (source == null && destination == null)
                return Enumerable.Empty<(string, (CompareStatus, StorageEntryMetadataV14))>();

            string prefix = source != null ? source.Prefix.Value : destination!.Prefix.Value;

            return CompareName(
                source != null ? Sanitize(source.Entries) : null,
                destination != null ? Sanitize(destination.Entries) : null)
                .Select(x => (prefix, x));
        }

        protected LookupDifferential CompareLookup(
            uint idSource, uint idDestination, PortableRegistry lookupSource, PortableRegistry lookupDestination)
        {
            var result = new LookupDifferential();

            var typeSource = lookupSource.Value.Single(x => x.Id.Value == idSource);
            var typeDestination = lookupDestination.Value.Single(x => x.Id.Value == idDestination);

            result.Id = CompareId(typeSource.Id, typeDestination.Id);
            result.Path = ComparePath(typeSource.Ty.Path, typeDestination.Ty.Path);
            result.Params = CompareParams(typeSource.Ty.TypeParams, typeDestination.Ty.TypeParams);
            result.LookupDifferentialType = CompareTypeDef(typeSource, typeDestination);
            result.Docs = CompareDocs(typeSource.Ty.Docs, typeDestination.Ty.Docs);

            return result;

        }

        private IList<(CompareStatus status, U32 id)> CompareId(U32 source, U32 destination)
        {
            var res = new List<(CompareStatus status, U32 id)>();
            if (source.Value != destination.Value)
            {
                res = new()
                {
                    (CompareStatus.Removed, source),
                    (CompareStatus.Added, destination)
                };
            }

            return res;
        }

        private IList<(CompareStatus status, BaseVec<Str> docs)> ComparePath(Base.Portable.Path source, Base.Portable.Path destination)
        {
            var res = new List<(CompareStatus status, BaseVec<Str> docs)>();
            if (!AreStringsEquals(source, destination))
            {
                res = new()
                {
                    (CompareStatus.Removed, source),
                    (CompareStatus.Added, destination)
                };
            }

            return res;
        }

        private IList<(CompareStatus status, BaseVec<TypeParameter> param)> CompareParams(BaseVec<TypeParameter> source, BaseVec<TypeParameter> destination)
        {
            var res = new List<(CompareStatus status, BaseVec<TypeParameter> param)>();

            if (!AreStringsEquals(source.Value.Select(x => x.Name), destination.Value.Select(x => x.Name)))
            {
                res = new()
                {
                    (CompareStatus.Removed, source),
                    (CompareStatus.Added, destination)
                };
            }

            return res;
        }

        private LookupDifferentialTypeDef CompareTypeDef(PortableType typeSource, PortableType typeDestination)
        {
            var result = new LookupDifferentialTypeDef();
            if (typeSource.Ty.TypeDef.Value != typeDestination.Ty.TypeDef.Value)
            {
                LookupDifferential.AddTypeDefAction(result, CompareStatus.Removed, typeSource);
                LookupDifferential.AddTypeDefAction(result, CompareStatus.Added, typeDestination);
            }
            else
            {
                // We got the same type, let's check if methods has been added or removed
                switch (typeSource.Ty.TypeDef.Value)
                {
                    case TypeDefEnum.Composite:
                        result.TypeComposite = CompareComposite(
                                (TypeDefComposite)typeSource.Ty.TypeDef.Value2,
                                (TypeDefComposite)typeDestination.Ty.TypeDef.Value2);
                        break;
                    case TypeDefEnum.Variant:
                        result.TypeVariant = new DifferentialVariant()
                        {
                            Elems = CompareVariant(
                                (TypeDefVariant)typeSource.Ty.TypeDef.Value2,
                                (TypeDefVariant)typeDestination.Ty.TypeDef.Value2)
                        };
                        break;
                    case TypeDefEnum.Sequence:
                        var sequenceSource = (TypeDefSequence)typeSource.Ty.TypeDef.Value2;
                        var sequenceDestination = (TypeDefSequence)typeDestination.Ty.TypeDef.Value2;
                        if (sequenceSource.ElemType != sequenceDestination.ElemType)
                        {
                            result.TypeSequence.Add((CompareStatus.Removed, sequenceSource));
                            result.TypeSequence.Add((CompareStatus.Added, sequenceDestination));
                        }
                        break;
                    case TypeDefEnum.Array:
                        result.TypeArray = CompareTypeBased(
                            (TypeDefArray)typeSource.Ty.TypeDef.Value2,
                            (TypeDefArray)typeDestination.Ty.TypeDef.Value2);
                        break;
                    case TypeDefEnum.Primitive:
                        var primSource = (BaseEnum<TypeDefPrimitive>)typeSource.Ty.TypeDef.Value2;
                        var primDestination = (BaseEnum<TypeDefPrimitive>)typeDestination.Ty.TypeDef.Value2;
                        if (primSource.Value != primDestination.Value)
                        {
                            result.TypePrimitive.Add((CompareStatus.Removed, primSource));
                            result.TypePrimitive.Add((CompareStatus.Added, primDestination));
                        }
                        break;
                    case TypeDefEnum.Compact:
                        result.TypeCompact = CompareTypeBased(
                            (TypeDefCompact)typeSource.Ty.TypeDef.Value2,
                            (TypeDefCompact)typeDestination.Ty.TypeDef.Value2);
                        break;
                    case TypeDefEnum.BitSequence:
                        result.TypeSequence = CompareTypeBased(
                            (TypeDefSequence)typeSource.Ty.TypeDef.Value2,
                            (TypeDefSequence)typeDestination.Ty.TypeDef.Value2);
                        break;
                    case TypeDefEnum.Tuple:
                        result.TypeTuple = CompareTuple(
                            (TypeDefTuple)typeSource.Ty.TypeDef.Value2,
                            (TypeDefTuple)typeDestination.Ty.TypeDef.Value2);
                        break;
                    default:
                        throw new MetadataException("TypeDefEnum not implemented !?");
                }

            }
            return result;
        }

        private IList<(CompareStatus, T)> CompareTypeBased<T>(T source, T destination)
            where T : BaseType, IMetadataType, new()
        {
            if (source.ElemType != destination.ElemType)
            {
                return new List<(CompareStatus, T)>()
                {
                    (CompareStatus.Removed, source),
                    (CompareStatus.Added, destination)
                };
            }

            return new List<(CompareStatus, T)>();
        }

        private DifferentialComposite CompareComposite(TypeDefComposite source, TypeDefComposite destination)
        {
            return new DifferentialComposite()
            {
                Elems = CompareType(Sanitize(source.Fields), Sanitize(destination.Fields))
            };
        }

        private IEnumerable<(CompareStatus, Variant)> CompareVariant(TypeDefVariant variantSource, TypeDefVariant variantDestination)
        {
            var res = CompareName(variantSource.TypeParam.Value, variantDestination.TypeParam.Value);

            return res;
        }

        private DifferentialTuple CompareTuple(TypeDefTuple source, TypeDefTuple destination)
        {
            return new DifferentialTuple()
            {
                Elems = CompareType(Sanitize(source.Fields), Sanitize(destination.Fields))
            };
        }

        private IList<(CompareStatus status, BaseVec<Str> docs)> CompareDocs(BaseVec<Str> source, BaseVec<Str> typeDestination)
        {
            var res = new List<(CompareStatus status, BaseVec<Str> docs)>();
            if (!AreStringsEquals(source, typeDestination))
            {
                res = new()
                {
                    (CompareStatus.Removed, source),
                    (CompareStatus.Added, typeDestination)
                };
            }
            return res;
        }
        #endregion
    }
}
