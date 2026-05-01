using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SewerMenu.GameAssemblyVerifier;

internal static class Program
{
    private const string DefaultGamePath = @"C:\Program Files (x86)\Steam\steamapps\common\Schedule I";

    private static readonly BindingFlags MemberFlags =
        BindingFlags.Public |
        BindingFlags.NonPublic |
        BindingFlags.Instance |
        BindingFlags.Static |
        BindingFlags.FlattenHierarchy;

    private static int Main(string[] args)
    {
        var options = Options.Parse(args);

        if (options.ShowHelp)
        {
            PrintHelp();
            return 0;
        }

        var gamePath = options.GamePath
            ?? Environment.GetEnvironmentVariable("SCHEDULE_I_PATH")
            ?? DefaultGamePath;

        var il2CppPath = Path.Combine(gamePath, "MelonLoader", "Il2CppAssemblies");
        var assemblyCSharpPath = Path.Combine(il2CppPath, "Assembly-CSharp.dll");
        var gameAssemblyPath = Path.Combine(gamePath, "GameAssembly.dll");
        var metadataPath = Path.Combine(gamePath, "Schedule I_Data", "il2cpp_data", "Metadata", "global-metadata.dat");

        if (!File.Exists(assemblyCSharpPath))
        {
            Console.Error.WriteLine($"ERROR: Could not find Assembly-CSharp.dll at '{assemblyCSharpPath}'.");
            Console.Error.WriteLine("Start Schedule I once with MelonLoader after updating the game so Il2CppAssemblies are regenerated.");
            return 2;
        }

        try
        {
            using var context = CreateMetadataContext(gamePath, il2CppPath);
            var assemblies = LoadSearchAssemblies(context, il2CppPath);

            PrintAssemblyInfo(assemblyCSharpPath, gameAssemblyPath, metadataPath);

            if (!string.IsNullOrWhiteSpace(options.ListFilter))
            {
                ListTypes(assemblies, options.ListFilter);
                return 0;
            }

            if (!string.IsNullOrWhiteSpace(options.MemberType))
            {
                PrintMembers(assemblies, options.MemberType);
                return 0;
            }

            var freshnessError = GetFreshnessError(assemblyCSharpPath, gameAssemblyPath, metadataPath);
            if (freshnessError != null && !options.AllowStale)
            {
                Console.Error.WriteLine("FAIL: MelonLoader generated IL2CPP assemblies are stale.");
                Console.Error.WriteLine(freshnessError);
                Console.Error.WriteLine("Start Schedule I once with MelonLoader installed so it regenerates MelonLoader\\Il2CppAssemblies, then run this verifier again.");
                Console.Error.WriteLine("Use --allow-stale only if you intentionally want to verify against old generated wrappers.");
                return 4;
            }

            var failures = VerifyRequirements(assemblies, BuildRequirements());

            if (failures.Count == 0)
            {
                Console.WriteLine("PASS: All required Schedule I IL2CPP types and members were found.");
                return 0;
            }

            Console.Error.WriteLine("FAIL: Missing or changed Schedule I IL2CPP API surface:");
            foreach (var failure in failures)
            {
                Console.Error.WriteLine($"  - {failure}");
            }

            return 1;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("ERROR: Verification failed unexpectedly.");
            Console.Error.WriteLine(ex);
            return 3;
        }
    }

    private static MetadataLoadContext CreateMetadataContext(string gamePath, string il2CppPath)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        AddDlls(paths, RuntimeEnvironment.GetRuntimeDirectory());
        AddDlls(paths, il2CppPath);
        AddDlls(paths, Path.Combine(gamePath, "MelonLoader", "net6"));

        return new MetadataLoadContext(new PathAssemblyResolver(paths));
    }

    private static List<Assembly> LoadSearchAssemblies(MetadataLoadContext context, string il2CppPath)
    {
        var names = new[]
        {
            "Assembly-CSharp.dll",
            "Assembly-CSharp-firstpass.dll",
            "Il2CppFishNet.Runtime.dll",
            "UnityEngine.CoreModule.dll"
        };

        var assemblies = new List<Assembly>();
        foreach (var name in names)
        {
            var path = Path.Combine(il2CppPath, name);
            if (!File.Exists(path)) continue;

            try
            {
                assemblies.Add(context.LoadFromAssemblyPath(path));
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"WARN: Could not load metadata from {name}: {ex.Message}");
            }
        }

        return assemblies;
    }

    private static List<string> VerifyRequirements(IReadOnlyList<Assembly> assemblies, IReadOnlyList<TypeRequirement> requirements)
    {
        var failures = new List<string>();

        foreach (var requirement in requirements)
        {
            var type = FindType(assemblies, requirement.TypeName);
            if (type == null)
            {
                failures.Add($"{requirement.TypeName}: type not found");
                continue;
            }

            foreach (var member in requirement.Members)
            {
                if (!HasMember(type, member))
                {
                    failures.Add($"{requirement.TypeName}.{member}");
                }
            }
        }

        return failures;
    }

    private static IReadOnlyList<TypeRequirement> BuildRequirements()
    {
        return new[]
        {
            Require("Il2CppScheduleOne.PlayerScripts.Player",
                Member.PropertyOrField("Local"),
                Member.PropertyOrField("IsOwner"),
                Member.PropertyOrField("IsLocalPlayer"),
                Member.PropertyOrField("Health"),
                Member.PropertyOrField("Energy"),
                Member.Method("GetEquippedItem", 0)),

            Require("Il2CppScheduleOne.PlayerScripts.PlayerMovement",
                Member.PropertyOrField("CurrentStaminaReserve"),
                Member.PropertyOrField("MoveSpeedMultiplier"),
                Member.PropertyOrField("StaticMoveSpeedMultiplier"),
                Member.PropertyOrField("IsGrounded"),
                Member.PropertyOrField("CurrentVehicle"),
                Member.Method("SetStamina", 2),
                Member.Method("SetResidualVelocity", 3)),

            Require("Il2CppScheduleOne.PlayerScripts.Health.PlayerHealth",
                Member.PropertyOrField("CurrentHealth"),
                Member.PropertyOrField("IsAlive"),
                Member.Method("SetHealth", 1)),

            Require("Il2CppScheduleOne.PlayerScripts.PlayerEnergy",
                Member.PropertyOrField("CurrentEnergy"),
                Member.Method("SetEnergy", 1),
                Member.Method("RestoreEnergy", 0)),

            Require("Il2CppScheduleOne.PlayerScripts.PlayerInventory",
                Member.PropertyOrField("Instance"),
                Member.PropertyOrField("InstanceExists"),
                Member.Method("AddItemToInventory", 1),
                Member.Method("CanItemFitInInventory", 2),
                Member.Method("GetAmountOfItem", 1)),

            Require("Il2CppScheduleOne.Money.MoneyManager",
                Member.PropertyOrField("cashBalance"),
                Member.PropertyOrField("onlineBalance"),
                Member.PropertyOrField("LifetimeEarnings"),
                Member.Method("ChangeCashBalance", 3),
                Member.Method("CreateOnlineTransaction", 4),
                Member.Method("GetNetWorth", 0)),

            Require("Il2CppScheduleOne.Levelling.LevelManager",
                Member.PropertyOrField("Tier"),
                Member.PropertyOrField("XP"),
                Member.PropertyOrField("TotalXP"),
                Member.PropertyOrField("XPToNextTier"),
                Member.PropertyOrField("Rank"),
                Member.Method("AddXP", 1),
                Member.Method("IncreaseTier", 0)),

            Require("Il2CppScheduleOne.GameTime.TimeManager",
                Member.PropertyOrField("CurrentTime"),
                Member.PropertyOrField("ElapsedDays"),
                Member.PropertyOrField("CurrentDay"),
                Member.PropertyOrField("IsNight"),
                Member.PropertyOrField("TimeSpeedMultiplier"),
                Member.Method("SetTime", 1),
                Member.Method("SetTimeAndSync", 1),
                Member.Method("SetTimeSpeedMultiplier", 1)),

            Require("Il2CppScheduleOne.ItemFramework.ItemDefinition",
                Member.PropertyOrField("ID"),
                Member.PropertyOrField("Name"),
                Member.PropertyOrField("Category"),
                Member.PropertyOrField("StackLimit"),
                Member.Method("GetDefaultInstance", 1)),

            Require("Il2CppScheduleOne.ItemFramework.ItemInstance",
                Member.PropertyOrField("Name"),
                Member.PropertyOrField("Equippable")),

            Require("Il2CppScheduleOne.ItemFramework.IntegerItemInstance",
                Member.PropertyOrField("Value"),
                Member.PropertyOrField("Quantity"),
                Member.Method("SetValue", 1),
                Member.Method("SetQuantity", 1)),

            Require("Il2CppScheduleOne.ItemFramework.QualityItemInstance",
                Member.PropertyOrField("Quality"),
                Member.Method("SetQuality", 1)),

            Require("Il2CppScheduleOne.ItemFramework.EQuality"),

            Require("Il2CppScheduleOne.ItemFramework.ItemPickup",
                Member.PropertyOrField("ItemToGive")),

            Require("Il2CppScheduleOne.Registry",
                Member.PropertyOrField("Instance"),
                Member.PropertyOrField("InstanceExists"),
                Member.PropertyOrField("ItemRegistry"),
                Member.PropertyOrField("ItemsAddedAtRuntime"),
                Member.Method("GetAllItems", 0)),

            Require("Il2CppScheduleOne.Equipping.Equippable_RangedWeapon",
                Member.PropertyOrField("Ammo"),
                Member.PropertyOrField("MagazineSize"),
                Member.PropertyOrField("weaponItem"),
                Member.PropertyOrField("CanReload"),
                Member.Method("CanFire", 1),
                Member.Method("Fire", 0),
                Member.Method("Reload", 0)),

            Require("Il2CppScheduleOne.NPCs.NPC",
                Member.PropertyOrField("Movement"),
                Member.PropertyOrField("Health"),
                Member.PropertyOrField("FirstName"),
                Member.PropertyOrField("IsConscious")),

            Require("Il2CppScheduleOne.Police.PoliceOfficer",
                Member.PropertyOrField("Suspicion"),
                Member.PropertyOrField("AutoDeactivate"),
                Member.Method("Deactivate", 0)),

            Require("Il2CppScheduleOne.Property.Property",
                Member.PropertyOrField("PropertyName"),
                Member.PropertyOrField("PropertyCode"),
                Member.PropertyOrField("IsOwned"),
                Member.PropertyOrField("Price")),

            Require("Il2CppScheduleOne.Property.PropertyManager",
                Member.Method("GetProperty", 1)),

            Require("Il2CppScheduleOne.Product.ProductDefinition",
                Member.PropertyOrField("ID"),
                Member.PropertyOrField("Name")),

            Require("Il2CppScheduleOne.Product.ProductManager",
                Member.PropertyOrField("AllProducts"),
                Member.Method("DiscoverProduct", 1)),

            Require("Il2CppScheduleOne.Growing.Plant",
                Member.PropertyOrField("IsFullyGrown"),
                Member.Method("SetNormalizedGrowthProgress", 1)),

            Require("Il2CppScheduleOne.Growing.ShroomColony",
                Member.PropertyOrField("IsFullyGrown"),
                Member.Method("SetFullyGrown", 0)),

            Require("Il2CppScheduleOne.Vehicles.LandVehicle",
                Member.PropertyOrField("Color"),
                Member.PropertyOrField("IsPlayerOwned"),
                Member.Method("GetVehicleData", 0),
                Member.Method("ApplyColor", 1)),

            Require("Il2CppScheduleOne.Vehicles.VehicleManager",
                Member.PropertyOrField("VehiclePrefabs"),
                Member.Method("SpawnAndReturnVehicle", 4)),

            Require("Il2CppScheduleOne.Vehicles.Modification.EVehicleColor"),

            Require("Il2CppScheduleOne.Vehicles.Modification.VehicleColors",
                Member.PropertyOrField("Instance"),
                Member.Method("GetColorName", 1)),

            Require("Il2CppScheduleOne.Economy.Dealer",
                Member.PropertyOrField("FirstName"),
                Member.PropertyOrField("IsRecruited")),

            Require("Il2CppScheduleOne.Economy.Customer",
                Member.PropertyOrField("NPC"),
                Member.PropertyOrField("IsAwaitingDelivery")),

            Require("Il2CppScheduleOne.ItemFramework.CashPickup",
                Member.PropertyOrField("Value")),
        };
    }

    private static TypeRequirement Require(string typeName, params Member[] members)
    {
        return new TypeRequirement(typeName, members);
    }

    private static bool HasMember(Type type, Member member)
    {
        return member.Kind switch
        {
            MemberKind.Property => type.GetProperty(member.Name, MemberFlags) != null,
            MemberKind.Field => type.GetField(member.Name, MemberFlags) != null,
            MemberKind.PropertyOrField => type.GetProperty(member.Name, MemberFlags) != null ||
                                          type.GetField(member.Name, MemberFlags) != null,
            MemberKind.Method => type.GetMethods(MemberFlags)
                .Any(method => method.Name == member.Name &&
                               (!member.ParameterCount.HasValue ||
                                method.GetParameters().Length == member.ParameterCount.Value)),
            _ => false
        };
    }

    private static Type? FindType(IEnumerable<Assembly> assemblies, string fullName)
    {
        foreach (var assembly in assemblies)
        {
            var type = assembly.GetType(fullName, throwOnError: false, ignoreCase: false);
            if (type != null) return type;
        }

        return null;
    }

    private static void ListTypes(IEnumerable<Assembly> assemblies, string filter)
    {
        Console.WriteLine($"Types matching '{filter}':");
        foreach (var typeName in assemblies.SelectMany(SafeGetTypes)
                     .Select(SafeTypeName)
                     .Where(typeName => typeName.Contains(filter, StringComparison.OrdinalIgnoreCase))
                     .OrderBy(typeName => typeName))
        {
            Console.WriteLine($"  {typeName}");
        }
    }

    private static void PrintMembers(IEnumerable<Assembly> assemblies, string typeName)
    {
        var type = FindType(assemblies, typeName);
        if (type == null)
        {
            Console.Error.WriteLine($"Type not found: {typeName}");
            return;
        }

        Console.WriteLine($"Members for {type.FullName}:");
        foreach (var property in type.GetProperties(MemberFlags).OrderBy(property => property.Name))
        {
            var canRead = property.GetMethod != null ? "get" : "";
            var canWrite = property.SetMethod != null ? "set" : "";
            Console.WriteLine($"  property {SafeTypeName(property.PropertyType)} {property.Name} {{{canRead}{(canRead.Length > 0 && canWrite.Length > 0 ? ";" : "")}{canWrite}}}");
        }

        foreach (var field in type.GetFields(MemberFlags).OrderBy(field => field.Name))
        {
            Console.WriteLine($"  field {SafeTypeName(field.FieldType)} {field.Name}");
        }

        foreach (var method in type.GetMethods(MemberFlags)
                     .Where(method => !method.IsSpecialName)
                     .OrderBy(method => method.Name)
                     .ThenBy(method => method.GetParameters().Length))
        {
            var parameters = string.Join(", ", method.GetParameters()
                .Select(parameter => $"{SafeTypeName(parameter.ParameterType)} {parameter.Name}"));
            Console.WriteLine($"  method {SafeTypeName(method.ReturnType)} {method.Name}({parameters})");
        }
    }

    private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.Where(type => type != null)!;
        }
    }

    private static string SafeTypeName(Type type)
    {
        try
        {
            return type.FullName ?? type.Name;
        }
        catch
        {
            try
            {
                return type.Name;
            }
            catch
            {
                return "<unreadable type>";
            }
        }
    }

    private static string? GetFreshnessError(string generatedAssemblyPath, string gameAssemblyPath, string metadataPath)
    {
        var generatedAssembly = new FileInfo(generatedAssemblyPath);
        var sources = new[]
        {
            new FileInfo(gameAssemblyPath),
            new FileInfo(metadataPath)
        }.Where(file => file.Exists).ToArray();

        if (!generatedAssembly.Exists || sources.Length == 0)
        {
            return null;
        }

        var newestSource = sources.OrderByDescending(file => file.LastWriteTimeUtc).First();
        if (generatedAssembly.LastWriteTimeUtc >= newestSource.LastWriteTimeUtc.AddMinutes(-1))
        {
            return null;
        }

        return $"Generated Assembly-CSharp.dll is from {generatedAssembly.LastWriteTime:yyyy-MM-dd HH:mm:ss}, " +
               $"but {newestSource.Name} is from {newestSource.LastWriteTime:yyyy-MM-dd HH:mm:ss}.";
    }

    private static void PrintAssemblyInfo(string assemblyPath, string gameAssemblyPath, string metadataPath)
    {
        var info = FileVersionInfo.GetVersionInfo(assemblyPath);
        var file = new FileInfo(assemblyPath);

        Console.WriteLine("Schedule I assembly verification");
        Console.WriteLine($"Assembly: {assemblyPath}");
        Console.WriteLine($"Generated wrapper modified: {file.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        PrintSourceTimestamp("GameAssembly.dll", gameAssemblyPath);
        PrintSourceTimestamp("global-metadata.dat", metadataPath);
        if (!string.IsNullOrWhiteSpace(info.FileVersion))
        {
            Console.WriteLine($"File version: {info.FileVersion}");
        }
        Console.WriteLine();
    }

    private static void PrintSourceTimestamp(string label, string path)
    {
        var file = new FileInfo(path);
        if (file.Exists)
        {
            Console.WriteLine($"{label} modified: {file.LastWriteTime:yyyy-MM-dd HH:mm:ss}");
        }
    }

    private static void AddDlls(HashSet<string> paths, string directory)
    {
        if (!Directory.Exists(directory)) return;

        foreach (var path in Directory.EnumerateFiles(directory, "*.dll"))
        {
            paths.Add(path);
        }
    }

    private static void PrintHelp()
    {
        Console.WriteLine("Usage:");
        Console.WriteLine("  dotnet run --project tools/SewerMenu.GameAssemblyVerifier -- [game path]");
        Console.WriteLine("  dotnet run --project tools/SewerMenu.GameAssemblyVerifier -- --list Player");
        Console.WriteLine();
        Console.WriteLine("If no game path is supplied, SCHEDULE_I_PATH or the default Steam install path is used.");
    }

    private sealed record Options(string? GamePath, string? ListFilter, string? MemberType, bool ShowHelp, bool AllowStale)
    {
        public static Options Parse(string[] args)
        {
            string? gamePath = null;
            string? listFilter = null;
            string? memberType = null;
            var showHelp = false;
            var allowStale = false;

            for (var i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg == "--help" || arg == "-h")
                {
                    showHelp = true;
                }
                else if (arg == "--list" && i + 1 < args.Length)
                {
                    listFilter = args[++i];
                }
                else if (arg == "--members" && i + 1 < args.Length)
                {
                    memberType = args[++i];
                }
                else if (arg == "--allow-stale")
                {
                    allowStale = true;
                }
                else if (!arg.StartsWith("-", StringComparison.Ordinal))
                {
                    gamePath = arg;
                }
            }

            return new Options(gamePath, listFilter, memberType, showHelp, allowStale);
        }
    }

    private sealed record TypeRequirement(string TypeName, IReadOnlyList<Member> Members);

    private sealed record Member(MemberKind Kind, string Name, int? ParameterCount = null)
    {
        public static Member Property(string name) => new(MemberKind.Property, name);

        public static Member Field(string name) => new(MemberKind.Field, name);

        public static Member PropertyOrField(string name) => new(MemberKind.PropertyOrField, name);

        public static Member Method(string name, int parameterCount) => new(MemberKind.Method, name, parameterCount);

        public override string ToString()
        {
            return Kind == MemberKind.Method
                ? $"{Name}({ParameterCount} params) missing"
                : $"{Name} missing";
        }
    }

    private enum MemberKind
    {
        Property,
        Field,
        PropertyOrField,
        Method
    }
}
