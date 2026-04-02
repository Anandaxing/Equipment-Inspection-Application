/*
ASSUMPTION:
- Operator stays assigned (persistent assignment).
The operator is responsible for that asset long-term. 
Performing maintenance is just one of their regular tasks, so they stay assigned after.

- Each Asset only has one Operator
But, one Operator can handle many Assets

- For Power (IPowerable):
   - All Pumps use the same rated limits (480V / 124A)
   - All Conveyors use the same rated limits (380V / 60A)
   - Power values cannot exceed rated limits — any attempt throws an exception (but is currently irrelevant)
*/
namespace FinalProject2
{
    public interface IPowerable // Like pumps & valves that use electric...
    {
        double Current { get; }
        double Voltage { get; }
        void ChangeSupplyPower(double volt, double ampere);
        bool CheckVoltageCurrentSafeThreshold(double volt, double ampere);
    }
    public interface IMaintainable // all assets, just an extra interface for demonstration
    {
        void PerformMaintenance();           // each asset type handles this differently
        List<string> MaintenanceHistory { get; }
    }
    public abstract class Asset : IMaintainable {
        private readonly string id;
        private string name;
        private string _maintenanceStatus;
        private Operator _assignedOperator;
        private List<string> _maintenanceHistory;
        public const string needMaintenance = "Needs Maintenance";
        public const string doneMaintenance  = "Maintenance Done";
        public Asset(string id, string name, string maintenanceStatus) {
            this.id = id;
            this.name = name;
            this._maintenanceStatus = maintenanceStatus;
            this._maintenanceHistory = new List<string>();
        }
        public string Id   => id;
        public string Name {
            get => name;
            set => name = value;
        }
        public string MaintenanceStatus {
            get => _maintenanceStatus;
            set
            {
                if (value == needMaintenance || value == doneMaintenance)
                    _maintenanceStatus = value;
                else
                    throw new ArgumentException($"Invalid maintenance status: '{value}'");
            }
        }
        public Operator AssignedOperator => _assignedOperator;
        public List<string> MaintenanceHistory => _maintenanceHistory;
        public abstract void PerformMaintenance();
        protected void LogMaintenance(string detail)
        {
            string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm}] {detail}";
            _maintenanceHistory.Add(entry);
            MaintenanceStatus = doneMaintenance;
        } 
        public void AssignOperator(Operator op) {
            _assignedOperator = op;
            op.AddAsset(this); 
        }
        public void UnassignOperator() {
            if (_assignedOperator == null) return;
            _assignedOperator.RemoveAsset(this);
            _assignedOperator = null;
        }
        public void DisplayBasicDetails() {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  [{GetType().Name}]");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($" Id: [{id}]");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($" Name: [{name}]");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" Status: {_maintenanceStatus}\n");


            // Console.WriteLine($"  [{GetType().Name}] Id: {id} | Name: {name} | Status: {_maintenanceStatus}");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"    Operator : {(_assignedOperator != null ? _assignedOperator.Name : "(none)")}");
            Console.WriteLine($"    History  : {_maintenanceHistory.Count} record(s)");
            Console.ResetColor();
        }
        public void DisplayHistory() {
            Console.WriteLine($"\nMaintenance history for {name}:");
            if (_maintenanceHistory.Count == 0) { Console.WriteLine("  No records yet."); return; }
            foreach (var entry in _maintenanceHistory)
                Console.WriteLine("  " + entry);
        } // not overriden
        public abstract void ShowAllMetrics(); // will be overriden
    }

    // ─────────────────────────────────────────
    // ASSET SUBCLASSES
    // ─────────────────────────────────────────

    public class Pump : Asset, IPowerable
    {
        private double current, voltage;
        private const double ratedVoltage = 480;
        private const double ratedCurrent = 124;
        private double flowRate;
        private double pressure;

        public Pump(string id, string name, string maintenanceStatus,
                    double current, double voltage, double flowRate, double pressure)
            : base(id, name, maintenanceStatus)
        {
            this.current  = current;
            this.voltage  = voltage;
            this.flowRate = flowRate;
            this.pressure = pressure;
        }

        public double Current => current;
        public double Voltage => voltage;
        
        // not used but demonstrates interface usage
        public void ChangeSupplyPower(double volt, double ampere) // from IPowerable, check first if rated V & A is safe
        {
            if (CheckVoltageCurrentSafeThreshold(volt, ampere))
            {
                this.voltage = volt;
                this.current = ampere;
                Console.WriteLine($"  Power updated: {voltage}V, {current}A.");
            }
            else
            {
                throw new InvalidOperationException("Cannot supply power outside the safe range.");
            }
        }
        public bool CheckVoltageCurrentSafeThreshold(double volt, double ampere)
            => volt <= ratedVoltage && ampere <= ratedCurrent; 
        public override void PerformMaintenance()
        {
            Console.WriteLine($"  [Pump] Inspecting seals, checking flow rate ({flowRate} L/min) and pressure ({pressure} bar)...");
            Console.WriteLine($"  [Pump] Verifying power within rated limits (≤{ratedVoltage}V / ≤{ratedCurrent}A)...");
            Console.WriteLine($"  [Pump] Maintenance complete.");
            LogMaintenance($"Pump inspection — Flow: {flowRate} L/min, Pressure: {pressure} bar, Power OK: {CheckVoltageCurrentSafeThreshold(voltage, current)}");
        }
        public override void ShowAllMetrics()
        {
            Console.WriteLine($"  Rated Voltage  : {ratedVoltage} V");
            Console.WriteLine($"  Rated Current  : {ratedCurrent} A");
            Console.WriteLine($"  Supply Voltage : {Voltage} V");
            Console.WriteLine($"  Supply Current : {Current} A");
            Console.WriteLine($"  Flow Rate      : {flowRate} L/min");
            Console.WriteLine($"  Pressure       : {pressure} bar");
            Console.WriteLine($"  Safe Threshold : {CheckVoltageCurrentSafeThreshold(Voltage, Current)}");
        }
    }
    public class Conveyor : Asset, IPowerable
    {
        private double beltSpeed;
        private bool isRunning;
        private double current, voltage;
        private const double ratedVoltage = 380;
        private const double ratedCurrent = 60;

        public Conveyor(string id, string name, string maintenanceStatus,
                        double beltSpeed, double current = 40, double voltage = 380)
            : base(id, name, maintenanceStatus)
        {
            this.beltSpeed = beltSpeed;
            this.isRunning = false;
            this.current   = current;
            this.voltage   = voltage;
        }

        public double BeltSpeed { get => beltSpeed; set => beltSpeed = value; }
        public bool   IsRunning { get => isRunning; set => isRunning = value; }
        public double Current   => current;
        public double Voltage   => voltage;

        public void ChangeSupplyPower(double volt, double ampere) // not used but demonstrates interface usage
        {
            if (CheckVoltageCurrentSafeThreshold(volt, ampere))
            {
                this.voltage = volt;
                this.current = ampere;
                Console.WriteLine($"  Power updated: {voltage}V, {current}A.");
            }
            else
            {
                throw new InvalidOperationException("Cannot supply power outside the safe range.");
            }
        }

        public bool CheckVoltageCurrentSafeThreshold(double volt, double ampere)
            => volt <= ratedVoltage && ampere <= ratedCurrent;

        // Conveyor-specific maintenance: belt tension and alignment check
        public override void PerformMaintenance()
        {
            Console.WriteLine($"  [Conveyor] Checking belt tension and alignment at {beltSpeed:F2} m/s...");
            Console.WriteLine($"  [Conveyor] Lubricating rollers, inspecting motor...");
            Console.WriteLine($"  [Conveyor] Maintenance complete.");
            LogMaintenance($"Conveyor inspection — Belt speed: {beltSpeed:F2} m/s, Running: {isRunning}");
        }

        public override void ShowAllMetrics()
        {
            Console.WriteLine($"  Belt Speed  : {beltSpeed:F2} m/s");
            Console.WriteLine($"  Running     : {isRunning}");
            Console.WriteLine($"  Voltage     : {voltage} V");
            Console.WriteLine($"  Current     : {current} A");
            Console.WriteLine($"  Safe Threshold : {CheckVoltageCurrentSafeThreshold(Voltage, Current)}");
        }
    }

    public class Valve : Asset
    {
        private bool isOpen;
        private double flowCoefficient; // Cv value
        public Valve(string id, string name, string maintenanceStatus,
                     bool isOpen, double flowCoefficient)
            : base(id, name, maintenanceStatus)
        {
            this.isOpen          = isOpen;
            this.flowCoefficient = flowCoefficient;
        }
        public bool   IsOpen          { get => isOpen;          set => isOpen = value; }
        public double FlowCoefficient { get => flowCoefficient; set => flowCoefficient = value; }
        public override void PerformMaintenance()
        {
            Console.WriteLine($"  [Valve] Testing actuator response and checking for leaks...");
            Console.WriteLine($"  [Valve] Flow coefficient (Cv): {flowCoefficient}. Valve is {(isOpen ? "OPEN" : "CLOSED")}.");
            Console.WriteLine($"  [Valve] Maintenance complete.");
            LogMaintenance($"Valve inspection — Open: {(isOpen ? "OPEN" : "CLOSED")}, Cv: {flowCoefficient}");
        }
        public override void ShowAllMetrics()
        {
            Console.WriteLine($"  State            : {(isOpen ? "Open" : "Closed")}");
            Console.WriteLine($"  Flow Coefficient : {flowCoefficient}");
        }
    }

    // ─────────────────────────────────────────
    // OPERATOR
    // ─────────────────────────────────────────

    public class Operator
    {
        private readonly string id;
        private string name;
        private string specialization;
        private List<Asset> _assignedAssets;

        public Operator(string id, string name, string specialization)
        {
            this.id             = id;
            this.name           = name;
            this.specialization = specialization;
            _assignedAssets     = new List<Asset>();
        }

        public string Id => id;
        public string Name { get => name; set => name = value; }
        public string Specialization { get => specialization; set => specialization = value; }

        public IReadOnlyList<Asset> AssignedAssets => _assignedAssets.AsReadOnly(); 
        public void AddAsset(Asset asset) => _assignedAssets.Add(asset);
        public void RemoveAsset(Asset asset) => _assignedAssets.Remove(asset);

        public void DisplayDetails()
        {
            Console.Write($"└─┤");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($" [{id}]");
            
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($" {name}");

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($" Specialization: {specialization}");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($" Assets assigned: {_assignedAssets.Count}\n");
            Console.ResetColor();
            // Console.WriteLine($"  [{id}] {name} | Specialization: {specialization} | Assets assigned: {_assignedAssets.Count}");
            foreach (var a in _assignedAssets)
                Console.WriteLine($"    -> {a.Id}: {a.Name}");
        }
    }

    // ─────────────────────────────────────────
    // SYSTEM CONTROLLER
    // ─────────────────────────────────────────
    /// <summary>
    /// Allow for all classes to interact, also handles UI
    /// </summary>
    public class SystemController
    {
        private List<Asset> _assets = new List<Asset>();
        private List<Operator> _operators = new List<Operator>();

        public void WelcomePage()
        {
            string banner = @"
╔══════════════════════════════════════════════════════════════════════════════════╗
║   _____   ______ _______ _____   ____  _   _           _____                     ║
║   |  __ \|  ____|__   __|  __ \ / __ \| \ | |   /\    / ____|     PETRONAS       ║
║   | |__) | |__     | |  | |__) | |  | |  \| |  /  \  | (___       Equipment      ║
║   |  ___/|  __|    | |  |  _  /| |  | | . ` | / /\ \  \___ \      Management     ║
║   | |    | |____   | |  | | \ \| |__| | |\  |/ ____ \ ____) |     Platform       ║
║   |_|    |______|  |_|  |_|  \_\\____/|_| \_/_/    \_\_____/      v1.0.0         ║
╚══════════════════════════════════════════════════════════════════════════════════╝";
            string text = "  [System]: Hey there, Welcome to PETRONAS equipment management application";
            string typing_text = ""; // typing text string container
            Console.ForegroundColor = ConsoleColor.Cyan; // switch to cyan color
            Console.WriteLine(banner); // print banner
            Console.ResetColor(); // switch back to default color (white)
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Black; 
            foreach (var character in text) // Typing test 
            {
                typing_text += character;
                Console.Write($"\r{typing_text}"); // printing typing text
                Thread.Sleep(10); // Delay for 10 milisecond
            }
            Console.ResetColor();
            Console.WriteLine("");
        }
        public void Run()
        {
            SeedData(); // when run, starting data is supplied for easy testing
            bool running = true;
            WelcomePage();
            while (running)
            {
                PrintHeader("EQUIPMENT MANAGEMENT, MAINTENANCE & INSPECTION SYSTEM");
                Console.ForegroundColor = ConsoleColor.Red; // switch to yellow color
                Console.WriteLine("┌─┤ IMPORTANT ├───────────────────────────────────────────┐");
                Console.WriteLine("│  Keep in mind equipments are also known as assets       │");
                Console.WriteLine("└─────────────────────────────────────────────────────────┘");
                Console.ResetColor(); // switch back to default color (white)
                Console.WriteLine("├─→ [1] View all assets");
                Console.WriteLine("├─→ [2] View all operators");
                Console.WriteLine("├─→ [3] Add asset");
                Console.WriteLine("├─→ [4] Remove asset");
                Console.WriteLine("├─→ [5] Assign operator to asset");
                Console.WriteLine("├─→ [6] Unassign operator from asset");
                Console.WriteLine("├─→ [7] Perform maintenance on asset");
                Console.WriteLine("├─→ [8] View asset metrics");
                Console.WriteLine("├─→ [9] View maintenance history");
                Console.WriteLine("├─→ [10] Operate assets (currently unusable, intended for changing power supply, \n│      on/off conveyor or open/close valve, irrelevant to the requirements)");
                Console.WriteLine("└─→ [0] Exit");
                Console.ForegroundColor = ConsoleColor.Yellow; // switch to yellow color
                Console.Write("\n  Select option: ");
                Console.ResetColor(); // switch back to default color (white)
                string input = Console.ReadLine()?.Trim() ?? "";
                try
                {
                    switch (input)
                    {
                        case "1": ViewAllAssets();           break;
                        case "2": ViewAllOperators();        break;
                        case "3": AddAssetMenu();            break;
                        case "4": RemoveAssetMenu();         break;
                        case "5": AssignOperatorMenu();      break;
                        case "6": UnassignOperatorMenu();    break;
                        case "7": PerformMaintenanceMenu();  break;
                        case "8": ViewAssetMetricsMenu();    break;
                        case "9": ViewHistoryMenu();         break;
                        case "0": running = false;           break;
                        default:
                            Console.ForegroundColor = ConsoleColor.Red;
                            Console.WriteLine("\n  Invalid option. Please enter a number from the menu.");
                            Console.ResetColor();
                            break;
                    }
                }
                catch (KeyNotFoundException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n  ERROR: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n  ERROR: {ex.Message}");
                }
                catch (ArgumentException ex)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"\n  ERROR: {ex.Message}");
                }
                Console.ResetColor();
                if (running)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write("\n  Press Enter to continue...");
                    Console.ResetColor();
                    Console.ReadLine();
                    Console.Clear(); // clear screen for clean UI
                }
            }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n  System shut down. Goodbye.");
            Console.ResetColor();
        }
        private void ViewAllAssets()
        {
            PrintHeader("ALL ASSETS");
            if (_assets.Count == 0) { Console.WriteLine("  No assets registered."); return; }
            foreach (var asset in _assets)
                asset.DisplayBasicDetails();
        }

        private void ViewAllOperators()
        {
            PrintHeader("ALL OPERATORS");
            if (_operators.Count == 0) { Console.WriteLine("  No operators registered."); return; }
            foreach (var op in _operators)
                op.DisplayDetails();
        }

        private void AddAssetMenu()
        {
            PrintHeader("ADD ASSET");
            Console.WriteLine("  Asset type:");
            Console.WriteLine("  [1] Pump");
            Console.WriteLine("  [2] Conveyor");
            Console.WriteLine("  [3] Valve");
            Console.Write("  Select: ");
            string type = Console.ReadLine()?.Trim() ?? "";
            

            Console.Write("  Enter ID   : "); string id   = Console.ReadLine()?.Trim() ?? "";
            if (!System.Text.RegularExpressions.Regex.IsMatch(id, @"^A-\d{3}$"))
                throw new ArgumentException("ID must follow the format A-xxx (e.g. A-001).");
            /* note for the pattern (invalid asset ID checking)
                ^ — must start here
                A- — literal "A-"
                \d{3} — exactly 3 digits
                $ — must end here
             */
            
            if (_assets.Exists(a => a.Id == id)) // to prevent duplicates 🙏😭
                throw new InvalidOperationException($"An asset with ID '{id}' already exists.");
            
            Console.Write("  Enter Name : "); string name = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                throw new ArgumentException("ID and Name cannot be empty.");



            // based on user input, we choose to instantiate which type of asset, and then prompt them to input values
            Asset newAsset;

            switch (type)
            {
                case "1":
                    Console.Write("  Flow Rate (L/min) : "); double flowRate = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("  Pressure (bar)    : "); double pressure = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("  Voltage (V)       : "); double voltage  = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("  Current (A)       : "); double current  = double.Parse(Console.ReadLine() ?? "0");
                    newAsset = new Pump(id, name, Asset.needMaintenance, current, voltage, flowRate, pressure);
                    break;

                case "2":
                    Console.Write("  Belt Speed (m/s) : "); double beltSpeed = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("  Voltage (V)      : "); double cVoltage  = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("  Current (A)      : "); double cCurrent  = double.Parse(Console.ReadLine() ?? "0");
                    newAsset = new Conveyor(id, name, Asset.needMaintenance, beltSpeed, cCurrent, cVoltage);
                    break;

                case "3":
                    Console.Write("  Flow Coefficient (Cv) : "); double cv = double.Parse(Console.ReadLine() ?? "0");
                    Console.Write("  Is Open? (y/n)        : "); bool isOpen = (Console.ReadLine()?.Trim().ToLower() == "y");
                    newAsset = new Valve(id, name, Asset.needMaintenance, isOpen, cv);
                    break;

                default:
                    throw new ArgumentException("Invalid asset type selected.");
            }

            _assets.Add(newAsset);
            Console.WriteLine($"\n  Asset '{name}' ({newAsset.GetType().Name}) added with ID: {id}");
        }

        private void RemoveAssetMenu()
        {
            PrintHeader("REMOVE ASSET");
            ViewAllAssets();
            Console.Write("\n  Enter Asset ID to remove: ");
            string id = Console.ReadLine()?.Trim() ?? "";


            Asset asset = FindAsset(id);
            asset.UnassignOperator();
            _assets.Remove(asset);
            Console.WriteLine($"\n  Asset '{asset.Name}' removed successfully.");
        }

        private void AssignOperatorMenu()
        {
            PrintHeader("ASSIGN OPERATOR TO ASSET");
            ViewAllAssets();
            Console.Write("\n  Enter Asset ID    : ");
            string assetId = Console.ReadLine()?.Trim() ?? "";

            ViewAllOperators();
            Console.Write("\n  Enter Operator ID : ");
            string opId = Console.ReadLine()?.Trim() ?? "";

            Asset    asset = FindAsset(assetId);
            Operator op    = FindOperator(opId);

            if (asset.AssignedOperator != null)
                throw new InvalidOperationException($"Asset '{asset.Name}' already has operator '{asset.AssignedOperator.Name}' assigned. Unassign first.");

            asset.AssignOperator(op);
            Console.WriteLine($"\n  Operator '{op.Name}' assigned to '{asset.Name}'.");
        }

        private void UnassignOperatorMenu()
        {
            PrintHeader("UNASSIGN OPERATOR FROM ASSET");
            ViewAllAssets();
            Console.Write("\n  Enter Asset ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            Asset asset = FindAsset(id);

            if (asset.AssignedOperator == null)
                throw new InvalidOperationException($"Asset '{asset.Name}' has no operator assigned.");

            string opName = asset.AssignedOperator.Name;
            asset.UnassignOperator();
            Console.WriteLine($"\n  Operator '{opName}' unassigned from '{asset.Name}'.");
        }

        private void PerformMaintenanceMenu()
        {
            PrintHeader("PERFORM MAINTENANCE");
            ViewAllAssets();
            Console.Write("\n  Enter Asset ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            Asset asset = FindAsset(id);

            if (asset.AssignedOperator == null)
                throw new InvalidOperationException($"Asset '{asset.Name}' has no operator assigned. Assign an operator before performing maintenance.");

            Console.WriteLine($"\n  Performing maintenance on '{asset.Name}'...\n");
            asset.PerformMaintenance(); // polymorphic bcs each asset handles this differently
        }

        private void ViewAssetMetricsMenu()
        {
            PrintHeader("VIEW ASSET METRICS");
            ViewAllAssets();
            Console.Write("\n  Enter Asset ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            Asset asset = FindAsset(id);
            Console.WriteLine($"\n  Metrics for {asset.Name}:");
            asset.ShowAllMetrics(); // depends on type of assets like before
        }
        
        private void ViewHistoryMenu()
        {
            PrintHeader("MAINTENANCE HISTORY");
            ViewAllAssets();
            Console.Write("\n  Enter Asset ID: ");
            string id = Console.ReadLine()?.Trim() ?? "";

            Asset asset = FindAsset(id);
            asset.DisplayHistory();
        }

        // ── Helpers ────────────────────────────────────────────────
        // both uses concise arrow syntax
        private Asset FindAsset(string id) =>
            _assets.Find(a => a.Id == id)
            ?? throw new KeyNotFoundException($"No asset found with ID '{id}'.");

        private Operator FindOperator(string id) =>
            _operators.Find(o => o.Id == id)
            ?? throw new KeyNotFoundException($"No operator found with ID '{id}'.");
        
        private static void PrintHeader(string title) // NOTA: quick, reusable cantikkanF
        {
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"┌─────────────────────────────────────────────────────────┐");
            Console.Write($"│  {title}  ");
            for(int iteration = 0 ; iteration < 53-title.Length; iteration++)
            {
              Console.Write(" ");
            }
            Console.WriteLine($"│\n└─────────────────────────────────────────────────────────┘");
            Console.ResetColor();
        }

        // ── Seed data (for testing, instantiated instantly when program is opened) ────────────────────────────────

        private void SeedData()
        {
            _assets.Add(new Pump    ("A-001", "Coolant Pump",       Asset.needMaintenance, 100, 200, 80, 5));
            _assets.Add(new Conveyor("A-002", "Assembly Conveyor",  Asset.needMaintenance, 2.5));
            _assets.Add(new Valve   ("A-003", "Inlet Control Valve",Asset.needMaintenance, false, 30.0));

            _operators.Add(new Operator("OP-01", "Ananda", "Mechanical"));
            _operators.Add(new Operator("OP-02", "Hasif",   "Electrical"));
            _operators.Add(new Operator("OP-03", "Hafiz",   "Electrical"));
        }
    }

    // ─────────────────────────────────────────
    // MAIN PROGRAM
    // ─────────────────────────────────────────

    class Program
    {
        static void Main()
        {
            new SystemController().Run();
        }
    }
}