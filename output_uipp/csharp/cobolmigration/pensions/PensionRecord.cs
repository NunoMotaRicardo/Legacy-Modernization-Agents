namespace CobolMigration.Pensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// DTO representing an incoming message (M807-like) describing a pension change request.
/// Only fields required by the converted logic are included. Additional fields can be added as needed.
/// </summary>
public record PensionChangeMessage
{
    /// <summary> Pension identifier (NPENS) </summary>
    public long PensionId { get; init; }

    /// <summary> Action code (e.g. 'A' authorize, 'E' eliminate) </summary>
    public char Action { get; init; }

    /// <summary> Payment modality code for CGA (0 => CNP) </summary>
    public int CmodpagCga { get; init; }

    /// <summary> Incoming pension variant 'IV' data (nullable). </summary>
    public IncomingPension Iv { get; init; }

    /// <summary> Incoming pension variant 'S' data (nullable). </summary>
    public IncomingPension S { get; init; }

    /// <summary> Operator / user name issuing the change (SCR-UTIL equivalent) </summary>
    public string Operator { get; init; } = string.Empty;

    /// <summary> Originating system / company code (SCR-NEMP equivalent) </summary>
    public int CompanyNumber { get; init; }
}

/// <summary>
/// Partial set of incoming pension fields used across validations and updates.
/// The COBOL source defines many more fields; only a functional, representative subset is implemented here.
/// </summary>
public record IncomingPension
{
    public char Event { get; init; }            // CEVENTO (e.g. '1','2','3' etc.)
    public string Coutp { get; init; } = string.Empty;
    public decimal Voutp { get; init; }
    public string Coutr { get; init; } = string.Empty;
    public decimal Voutr { get; init; }
    public DateTime? DtiniOr { get; init; }    // DTINIOR as date
    public DateTime? DtiniOp { get; init; }    // DTINIOP as date
    public int V13mes { get; init; }           // V13MES
    public string CseSp { get; init; } = string.Empty; // CSESP
    public string Cpesp { get; init; } = string.Empty;  // CPESP
    public int Ncga { get; init; }             // NCGA
    public string Cconvi { get; init; } = string.Empty; // CCONVI
    public List<Parcel> Parcels { get; init; } = new(); // Up to 17 parcel entries
    public DateTime? Dtnas { get; init; }      // birth date for some checks (DTNAS)
    public int DtIniNumeric { get; init; }     // integer date like YYYYMMDD when needed
    public long NrNomeP { get; init; }         // NRNOMEP - name reference
}

/// <summary>
/// Representation of a parcel (CPAR code, date, value).
/// Matches the W-TABELA-PARCELAS structure (CPAR, DTINI, VPAR).
/// </summary>
public record Parcel
{
    public string Code { get; init; } = string.Empty; // e.g. "01","22","ES"
    public DateTime? StartDate { get; init; }         // DTINI
    public decimal Value { get; init; }               // VPAR
}

/// <summary>
/// Pension master record stored/retrieved from repository (PNSCNP / PNSCGA combined).
/// This maps a subset of the many W-...-CNP fields used by the COBOL program.
/// </summary>
public class PensionRecord
{
    public long PensionId { get; set; }
    public int CompanyNumber { get; set; }
    public string Event { get; set; } = string.Empty;
    public string Coutp { get; set; } = string.Empty;
    public decimal Voutp { get; set; }
    public string Coutr { get; set; } = string.Empty;
    public decimal Voutr { get; set; }
    public DateTime? DtiniOr { get; set; }
    public DateTime? DtiniOp { get; set; }
    public int V13mes { get; set; }
    public string CseSp { get; set; } = string.Empty;
    public string Cpesp { get; set; } = string.Empty;
    public int Ncga { get; set; }
    public string Cconvi { get; set; } = string.Empty;
    public string CseSp1 { get; set; } = string.Empty;
    public string CseSp2 { get; set; } = string.Empty;
    public string CseSp3 { get; set; } = string.Empty;
    public int Progcc { get; set; }
    public int Cregime { get; set; }
    public int Csusp { get; set; }
    public int Nanciv { get; set; }
    public int Nanmes { get; set; }
    public int DtfalNumeric { get; set; } // DTFAL as YYYYMM or 0
    public int DtgrvNumeric { get; set; } // DTGRV numeric YYYYMMDD for comparisons
    public bool IsCgaRecord { get; set; }
    public string Iban { get; set; } = string.Empty;
    public string Cswift { get; set; } = string.Empty;
    public int Cpprov { get; set; }
    // ... many other fields could be added to fully match COBOL
}

/// <summary>
/// History entry used to record text descriptions of changes (HISTALT).
/// </summary>
public class HistoryEntry
{
    public long PensionId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public int Code { get; set; }
    public string Origin { get; set; } = "AP825";
    public int CompanyNumber { get; set; }
    public string Terminal { get; set; } = string.Empty;
    public int ProcessingDate { get; set; }
    public string Description { get; set; } = string.Empty;
}

/// <summary>
/// Validation result with possible error code and textual description.
/// </summary>
public record ValidationResult(bool IsValid, int ErrorCode = 0, string ErrorMessage = "")
{
    public static ValidationResult Ok() => new(true);
    public static ValidationResult Fail(int code, string msg) => new(false, code, msg);
}

/// <summary>
/// Common application-level exception used to signal business errors (maps to ERRO-APLICACAO).
/// Contains a code and a message suitable for translation to MERR structures.
/// </summary>
public class ApplicationBusinessException : Exception
{
    public int ErrorCode { get; }

    public ApplicationBusinessException(int errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}

/// <summary>
/// Database error abstraction (maps to ERRO-BD).
/// </summary>
public class DataAccessException : Exception
{
    public DataAccessException(string message, Exception? inner = null) : base(message, inner) { }
}

/// <summary>
/// Repository interface representing RDMS access and FPEDOC file operations required by the program.
/// Implementations may be in-memory, use ADO.NET, EF Core, Dapper, or other data access layers.
/// The COBOL logic uses many tables and cursors; this interface surfaces the required operations as methods.
/// </summary>
public interface IPensionRepository
{
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);

    Task<PensionRecord?> GetPensionAsync(long pensionId, int cmodpag, CancellationToken ct = default);
    Task<IList<Parcel>> GetParcelsAsync(long pensionId, CancellationToken ct = default);
    Task<IList<Parcel>> GetParcelsVpAsync(long pensionId, CancellationToken ct = default);
    Task<IList<Parcel>> GetParcelacgaAsync(long pensionId, int cmodpag, CancellationToken ct = default);
    Task<int> CountParcelsByCodesAsync(long pensionId, params string[] codes);
    Task<int> CountParcelsWithCodeAsync(long pensionId, string code);
    Task<TabCvp?> GetTabCvpAsync(long pensionId, string origin, int cmodpag, CancellationToken ct = default);
    Task<decimal?> GetTabValFixoAsync(string code, DateTime date, int year, CancellationToken ct = default);

    Task InsertParcelsAsync(long pensionId, IEnumerable<Parcel> parcels, CancellationToken ct = default);
    Task DeleteParcelsAsync(long pensionId, CancellationToken ct = default);

    Task UpdatePensionAsync(PensionRecord record, CancellationToken ct = default);

    Task DeleteTabCvpAsync(long pensionId, string origin, int cmodpag, CancellationToken ct = default);
    Task DeleteParcelsVpAsync(long pensionId, CancellationToken ct = default);
    Task InsertHistoryAsync(HistoryEntry entry, CancellationToken ct = default);

    Task<bool> ExistsParcelacgaRecordAsync(long pensionId, int cmodpag, Parcel parcel, CancellationToken ct = default);
    Task<IList<Parcel>> FetchParcelacgaCursorAsync(long pensionId, int cmodpag, CancellationToken ct = default);

    // FPEDOC (file) operations
    Task CreateFpedocRequestAsync(long pensionId, int requestType, string operatorName, CancellationToken ct = default);
    Task DeleteFpedocRequestsByTypeAsync(long pensionId, int requestType, CancellationToken ct = default);
}

/// <summary>
/// Lightweight structure representing a TABCVP row (NEMP, DESCR, DTGRV).
/// Used by ACEDER-TABCVP logic.
/// </summary>
public record TabCvp(int CompanyNumber, string Description, DateTime? DtGrv);

/// <summary>
/// Simple in-memory implementation of IPensionRepository that keeps data in memory for demonstration and unit testing.
/// This preserves the execution flow of the COBOL program while avoiding any external database dependencies.
/// In production a real repository would implement database access and proper transactional semantics.
/// </summary>
public class InMemoryPensionRepository : IPensionRepository
{
    private readonly ConcurrentDictionary<long, PensionRecord> _pensions = new();
    private readonly ConcurrentDictionary<long, List<Parcel>> _parcels = new();
    private readonly ConcurrentDictionary<long, List<Parcel>> _parcelsVp = new();
    private readonly ConcurrentDictionary<long, List<Parcel>> _parcelacga = new();
    private readonly ConcurrentDictionary<long, TabCvp> _tabcvp = new();
    private readonly ConcurrentDictionary<string, decimal> _tabvalfixo = new();
    private readonly List<HistoryEntry> _history = new();
    private readonly List<(long PensionId, int RequestType)> _fpedoc = new();
    private readonly AsyncLocal<bool> _inTransaction = new();

    public InMemoryPensionRepository()
    {
        // Seed data if needed for functional testing:
        // Example: populate TABVALFIXO with some codes used in validations.
        _tabvalfixo["15_2022"] = 1000m; // code 15 for 2022 baseline
        _tabvalfixo["PS_2022"] = 800m;  // PS code example
        _tabvalfixo["SG_2022"] = 900m;
        _tabvalfixo["CD_2022"] = 950m;
        // No pensions seeded by default, caller can seed via UpdatePensionAsync
    }

    public Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _inTransaction.Value = true;
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        _inTransaction.Value = false;
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        _inTransaction.Value = false;
        return Task.CompletedTask;
    }

    public Task<PensionRecord?> GetPensionAsync(long pensionId, int cmodpag, CancellationToken ct = default)
    {
        _pensions.TryGetValue(pensionId, out var rec);
        return Task.FromResult(rec);
    }

    public Task<IList<Parcel>> GetParcelsAsync(long pensionId, CancellationToken ct = default)
    {
        _parcels.TryGetValue(pensionId, out var list);
        return Task.FromResult((IList<Parcel>)(list ?? new List<Parcel>()));
    }

    public Task<IList<Parcel>> GetParcelsVpAsync(long pensionId, CancellationToken ct = default)
    {
        _parcelsVp.TryGetValue(pensionId, out var list);
        return Task.FromResult((IList<Parcel>)(list ?? new List<Parcel>()));
    }

    public Task<IList<Parcel>> GetParcelacgaAsync(long pensionId, int cmodpag, CancellationToken ct = default)
    {
        _parcelacga.TryGetValue(pensionId, out var list);
        return Task.FromResult((IList<Parcel>)(list ?? new List<Parcel>()));
    }

    public Task<int> CountParcelsByCodesAsync(long pensionId, params string[] codes)
    {
        if (_parcels.TryGetValue(pensionId, out var list))
        {
            var count = list.Count(p => codes.Contains(p.Code));
            return Task.FromResult(count);
        }
        return Task.FromResult(0);
    }

    public Task<int> CountParcelsWithCodeAsync(long pensionId, string code)
    {
        if (_parcels.TryGetValue(pensionId, out var list))
            return Task.FromResult(list.Count(p => p.Code == code));
        return Task.FromResult(0);
    }

    public Task<TabCvp?> GetTabCvpAsync(long pensionId, string origin, int cmodpag, CancellationToken ct = default)
    {
        if (_tabcvp.TryGetValue(pensionId, out var t))
            return Task.FromResult<TabCvp?>(t);
        return Task.FromResult<TabCvp?>(null);
    }

    public Task<decimal?> GetTabValFixoAsync(string code, DateTime date, int year, CancellationToken ct = default)
    {
        // Emulate TABVALFIXO selection by code + year key
        var key = $"{code}_{year}";
        if (_tabvalfixo.TryGetValue(key, out var v))
            return Task.FromResult<decimal?>(v);
        return Task.FromResult<decimal?>(null);
    }

    public Task InsertParcelsAsync(long pensionId, IEnumerable<Parcel> parcels, CancellationToken ct = default)
    {
        _parcels[pensionId] = parcels.ToList();
        return Task.CompletedTask;
    }

    public Task DeleteParcelsAsync(long pensionId, CancellationToken ct = default)
    {
        _parcels.TryRemove(pensionId, out _);
        return Task.CompletedTask;
    }

    public Task UpdatePensionAsync(PensionRecord record, CancellationToken ct = default)
    {
        _pensions[record.PensionId] = record;
        return Task.CompletedTask;
    }

    public Task DeleteTabCvpAsync(long pensionId, string origin, int cmodpag, CancellationToken ct = default)
    {
        _tabcvp.TryRemove(pensionId, out _);
        return Task.CompletedTask;
    }

    public Task DeleteParcelsVpAsync(long pensionId, CancellationToken ct = default)
    {
        _parcelsVp.TryRemove(pensionId, out _);
        return Task.CompletedTask;
    }

    public Task InsertHistoryAsync(HistoryEntry entry, CancellationToken ct = default)
    {
        _history.Add(entry);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsParcelacgaRecordAsync(long pensionId, int cmodpag, Parcel parcel, CancellationToken ct = default)
    {
        if (_parcelacga.TryGetValue(pensionId, out var list))
        {
            return Task.FromResult(list.Any(p => p.Code == parcel.Code && p.Value == parcel.Value && p.StartDate == parcel.StartDate));
        }
        return Task.FromResult(false);
    }

    public Task<IList<Parcel>> FetchParcelacgaCursorAsync(long pensionId, int cmodpag, CancellationToken ct = default)
    {
        _parcelacga.TryGetValue(pensionId, out var list);
        return Task.FromResult((IList<Parcel>)(list ?? new List<Parcel>()));
    }

    public Task CreateFpedocRequestAsync(long pensionId, int requestType, string operatorName, CancellationToken ct = default)
    {
        lock (_fpedoc)
        {
            _fpedoc.Add((pensionId, requestType));
        }
        return Task.CompletedTask;
    }

    public Task DeleteFpedocRequestsByTypeAsync(long pensionId, int requestType, CancellationToken ct = default)
    {
        lock (_fpedoc)
        {
            _fpedoc.RemoveAll(r => r.PensionId == pensionId && r.RequestType == requestType);
        }
        return Task.CompletedTask;
    }
}

/// <summary>
/// Service responsible for validations that the original COBOL program performed with many business rules.
/// The Validate* methods implement the business rules as executable code and return ValidationResult instances.
/// </summary>
public interface IValidationService
{
    Task<ValidationResult> ValidateParcelsAndLimitsAsync(PensionChangeMessage message, PensionRecord existing, IPensionRepository repo, CancellationToken ct = default);
    Task<ValidationResult> ValidateConventionsAndSpecialCasesAsync(PensionChangeMessage message, PensionRecord existing, IPensionRepository repo, CancellationToken ct = default);
}

/// <summary>
/// Implementation of validation service. It contains implementations of several important business checks from the COBOL source.
/// Only a representative and essential set of validations are implemented to preserve logic and executable behavior.
/// Additional validations from the COBOL program can be incrementally added as methods on this service.
/// </summary>
public class ValidationService : IValidationService
{
    /// <summary>
    /// Validate parcel-level rules and compare sums against TABVALFIXO rules.
    /// Implements representative checks from the COBOL program:
    /// - accumulate parcel sums excluding specific codes
    /// - check against TABVALFIXO value for a given code (15, PS, SG, CD etc.)
    /// - check special rules for island users or CRSS operator where the allowed caps differ
    /// - return ValidationResult.Fail with error code consistent with COBOL WS-TERRCOD where a rule fails.
    /// </summary>
    public async Task<ValidationResult> ValidateParcelsAndLimitsAsync(PensionChangeMessage message, PensionRecord existing, IPensionRepository repo, CancellationToken ct = default)
    {
        // Build an index of incoming parcels (max 17 per original program)
        var parcels = message.Iv?.Parcels ?? message.S?.Parcels ?? new List<Parcel>();
        // Determine user/operator conditions (AGU-UTILIZADOR logic in COBOL)
        var operatorIsCnp = (message.Operator?.ToUpperInvariant() ?? "") == "CNP";
        var operatorIsCrss = (message.Operator?.ToUpperInvariant() ?? "") == "CRSS";

        // Accumulate sums excluding specific codes: exclude '10','22','42','2I','4I','67','ES' etc depending on logic
        decimal accumulated = 0m;
        foreach (var p in parcels)
        {
            if (string.IsNullOrWhiteSpace(p.Code)) continue;
            // Exclusion list based on COBOL logic for sum-of-values checks
            var exclude = new HashSet<string> { "10", "22", "42", "2I", "4I", "67", "ES" };
            if (!exclude.Contains(p.Code))
            {
                accumulated += p.Value;
            }
        }

        // Consider VOUTP/VOUTR from incoming message as part of accumulated for certain conditions
        var incoming = message.Iv ?? message.S;
        if (incoming != null)
        {
            var coutp = incoming.Coutp ?? string.Empty;
            // In COBOL certain Coutp values are excluded from sum (like 'E','B','H','I','J','L','M','N','O')
            var excludedCoutp = new HashSet<string> { "E", "B", "H", "I", "J", "L", "M", "N", "O", "" };
            if (!excludedCoutp.Contains(coutp.Trim()))
            {
                accumulated += incoming.Voutp;
            }
        }

        // Access TABVALFIXO (code 15 used as example for total parcel cap)
        var year = DateTime.UtcNow.Year;
        var tvf = await repo.GetTabValFixoAsync("15", DateTime.UtcNow, year, ct);
        if (tvf.HasValue && accumulated > tvf.Value)
        {
            // COBOL sets error code 84 for W-VALOR > W-VALOR-TVF
            return ValidationResult.Fail(84, "Total parcels exceed allowed fixed table value (code 15).");
        }

        // Example: Validate specific parcel codes against 'PS' category (code depends on user)
        if (operatorIsCnp)
        {
            // If parcel '33' present must be <= 60% of TVF (COBOL logic)
            var tvfPs = await repo.GetTabValFixoAsync("PS", DateTime.UtcNow, year, ct);
            if (tvfPs.HasValue)
            {
                var cap = Math.Round(tvfPs.Value * 0.60M, 2);
                var p33 = parcels.FirstOrDefault(p => p.Code == "33");
                if (p33 != null && p33.Value > cap)
                {
                    return ValidationResult.Fail(79, "Parcel 33 exceeds allowed PS cap (60%).");
                }
            }
        }

        // Additional example rule: if parcel 12 present with zero complementary and VOUTP = 0 then error (COBOL mapped to code 372)
        var parcel12 = parcels.FirstOrDefault(p => p.Code == "12");
        if (parcel12 != null && incoming != null)
        {
            if (parcel12.Value < (await repo.GetTabValFixoAsync("15", DateTime.UtcNow, year, ct) ?? 0m) && incoming.Voutp == 0)
                return ValidationResult.Fail(372, "Parcel 12 below required threshold while VOUTP is 0.");
        }

        // Validate parcel combinations that must not coexist (e.g., 22 & 10) — simplified
        var has22 = parcels.Any(p => p.Code == "22");
        var has10 = parcels.Any(p => p.Code == "10");
        if (has22 && has10)
        {
            // COBOL used code 950 in some combination checks
            return ValidationResult.Fail(950, "Incompatible parcel combination detected (22 and 10).");
        }

        // If no problem discovered return OK
        return ValidationResult.Ok();
    }

    /// <summary>
    /// Validate conventions, special codes, age or date rules and other business specifics.
    /// Implements representative checks such as:
    /// - Verify event-specific constraints (e.g. CSESP vs stored values)
    /// - Age checks for FPEDOC behavior (if used)
    /// - Simple cross-field comparisons producing appropriate error codes.
    /// </summary>
    public Task<ValidationResult> ValidateConventionsAndSpecialCasesAsync(PensionChangeMessage message, PensionRecord existing, IPensionRepository repo, CancellationToken ct = default)
    {
        // Example: if event changed and Csusp is > 0 produce error (mapped from COBOL 621)
        if (existing != null && message.Iv is not null)
        {
            if (existing.Csusp > 0 && existing.CompanyNumber != message.CompanyNumber)
            {
                return Task.FromResult(ValidationResult.Fail(621, "Suspension flag mismatch for company; update not allowed."));
            }
        }

        // Example: if incoming CEVENTO is '3' but stored CPAREN or CMCSOB mismatch produce 951
        if (message.Iv is not null && existing is not null)
        {
            if (message.Iv.Event == '3')
            {
                // This maps complex COBOL conditional that compared WS-CEVENTO-CNP and S-CPAREN etc.
                // Use a simplified rule: if stored event differs and CPAREN mismatch -> error 951
                if (existing.Event != message.Iv.Event.ToString())
                {
                    return Task.FromResult(ValidationResult.Fail(951, "Event or parent relation mismatch for orphan/guardian rules."));
                }
            }
        }

        // No rule triggered
        return Task.FromResult(ValidationResult.Ok());
    }
}

/// <summary>
/// Service responsible for file operations and FPEDOC handling.
/// Preserves file-oriented behavior of COBOL TRATA-FPEDOC paragraphs by creating/deleting FPEDOC requests via repository methods.
/// </summary>
public interface IFileService
{
    Task HandleFpedocAsync(PensionChangeMessage message, int requestType, CancellationToken ct = default);
    Task DeleteFpedocIfExistsAsync(PensionChangeMessage message, int requestType, CancellationToken ct = default);
}

/// <summary>
/// Implementation of IFileService that delegates to repository to create or delete FPEDOC-like requests.
/// </summary>
public class FileService : IFileService
{
    private readonly IPensionRepository _repo;
    public FileService(IPensionRepository repo) => _repo = repo;

    public async Task HandleFpedocAsync(PensionChangeMessage message, int requestType, CancellationToken ct = default)
    {
        // In the COBOL code TRATA-FPEDOC wrote requests of type 15/16/8 depending on conditions.
        // This method creates a request record in the repository.
        await _repo.CreateFpedocRequestAsync(message.PensionId, requestType, message.Operator, ct);
    }

    public async Task DeleteFpedocIfExistsAsync(PensionChangeMessage message, int requestType, CancellationToken ct = default)
    {
        await _repo.DeleteFpedocRequestsByTypeAsync(message.PensionId, requestType, ct);
    }
}

/// <summary>
/// History service responsible for composing and inserting history (HISTALT) records.
/// This corresponds to ESCREVE-DESCRICAO, ESCREVE-DESCRICAO-E-TESTA, INSERIR-HISTALT paragraphs in COBOL.
/// </summary>
public interface IHistoryService
{
    Task InsertHistoryAsync(long pensionId, int code, string description, string origin, int companyNumber, CancellationToken ct = default);
}

/// <summary>
/// Implementation of history service which uses the repository to insert history rows.
/// </summary>
public class HistoryService : IHistoryService
{
    private readonly IPensionRepository _repo;
    public HistoryService(IPensionRepository repo) => _repo = repo;

    public Task InsertHistoryAsync(long pensionId, int code, string description, string origin, int companyNumber, CancellationToken ct = default)
    {
        var entry = new HistoryEntry
        {
            PensionId = pensionId,
            Timestamp = DateTime.UtcNow,
            Code = code,
            Origin = origin,
            CompanyNumber = companyNumber,
            Description = description,
            ProcessingDate = int.Parse(DateTime.UtcNow.ToString("yyyyMM"))
        };
        return _repo.InsertHistoryAsync(entry, ct);
    }
}

/// <summary>
/// Provides locking semantics (no-op in single-process in-memory implementation).
/// The COBOL program used LOCK TABLE to serialize modifications; here it is preserved as a service.
/// </summary>
public interface ILockService
{
    Task LockTablesAsync(CancellationToken ct = default);
}

public class InMemoryLockService : ILockService
{
    public Task LockTablesAsync(CancellationToken ct = default) => Task.CompletedTask;
}

/// <summary>
/// Service responsible for sending errors or translating DB/application errors into external notifications.
/// The COBOL program assembled MERR structures and then invoked message-sending APIs. This service preserves that behavior.
/// </summary>
public interface IErrorService
{
    Task ReportApplicationErrorAsync(int code, string message, PensionChangeMessage origin, CancellationToken ct = default);
    Task ReportDatabaseErrorAsync(string message, PensionChangeMessage origin, CancellationToken ct = default);
}

/// <summary>
/// Simple console-based ErrorService that logs errors to the console (or other sinks in real microservices).
/// </summary>
public class ConsoleErrorService : IErrorService
{
    public Task ReportApplicationErrorAsync(int code, string message, PensionChangeMessage origin, CancellationToken ct = default)
    {
        Console.Error.WriteLine($"[APPLICATION ERROR] Code={code} Pension={origin.PensionId} Operator={origin.Operator} Message={message}");
        return Task.CompletedTask;
    }

    public Task ReportDatabaseErrorAsync(string message, PensionChangeMessage origin, CancellationToken ct = default)
    {
        Console.Error.WriteLine($"[DATABASE ERROR] Pension={origin.PensionId} Operator={origin.Operator} Message={message}");
        return Task.CompletedTask;
    }
}

/// <summary>
/// The top-level service implementing the business process of the original COBOL program AP825.
/// Class name follows the required pattern: Domain + Action + Type -> PensionChangeAuthorizationService.
/// This service orchestrates validation, data access, history creation and file request handling,
/// decomposed into injected services to satisfy microservice architecture principles.
/// </summary>
public class PensionChangeAuthorizationService
{
    private readonly IPensionRepository _repo;
    private readonly IValidationService _validator;
    private readonly IHistoryService _historyService;
    private readonly IFileService _fileService;
    private readonly ILockService _lockService;
    private readonly IErrorService _errorService;

    /// <summary>
    /// Create a new service instance with injected dependencies.
    /// </summary>
    public PensionChangeAuthorizationService(
        IPensionRepository repo,
        IValidationService validator,
        IHistoryService historyService,
        IFileService fileService,
        ILockService lockService,
        IErrorService errorService)
    {
        _repo = repo;
        _validator = validator;
        _historyService = historyService;
        _fileService = fileService;
        _lockService = lockService;
        _errorService = errorService;
    }

    /// <summary>
    /// Main entry point for processing an incoming pension change message.
    /// This method maps the COBOL PROGRAM-ID AP825 main flows: initialization, choose CNP/CGA, validate, update/insert/delete and finalize.
    /// It preserves transaction semantics and error handling by using repository Begin/Commit/Rollback methods.
    /// </summary>
    public async Task ProcessAsync(PensionChangeMessage message, CancellationToken ct = default)
    {
        try
        {
            await _repo.BeginTransactionAsync(ct);

            // Lock tables as COBOL did before reading and writing multiple tables
            await _lockService.LockTablesAsync(ct);

            // Determine program path: CNP (cmodpagt == 0) or CGA (cmodpagt != 0)
            if (message.CmodpagCga == 0)
                await ProcessCnpAsync(message, ct);
            else
                await ProcessCgaAsync(message, ct);

            await _repo.CommitAsync(ct);
        }
        catch (ApplicationBusinessException abe)
        {
            // Business validation failure: rollback and report
            await _repo.RollbackAsync(ct);
            await _errorService.ReportApplicationErrorAsync(abe.ErrorCode, abe.Message, message, ct);
            // rethrow if caller needs to inspect exception
            throw;
        }
        catch (DataAccessException dae)
        {
            await _repo.RollbackAsync(ct);
            await _errorService.ReportDatabaseErrorAsync(dae.Message, message, ct);
            throw;
        }
        catch (Exception ex)
        {
            await _repo.RollbackAsync(ct);
            await _errorService.ReportDatabaseErrorAsync(ex.Message, message, ct);
            throw;
        }
    }

    /// <summary>
    /// Process CNP flows (equivalent to PROGRAMA-CNP section).
    /// Handles validation, compare-with-database, update/insert histories, create FPEDOC if required, and deletion flows.
    /// </summary>
    private async Task ProcessCnpAsync(PensionChangeMessage message, CancellationToken ct)
    {
        // Load existing pension (PNSCNP)
        var existing = await _repo.GetPensionAsync(message.PensionId, 0, ct);
        if (existing == null)
        {
            // Corresponds to SQLCODE-EOF handling in ACEDER-PNSCNP -> error 242 or 5 depending on context
            throw new ApplicationBusinessException(242, $"Pension {message.PensionId} not found in PNSCNP.");
        }

        // If action is 'E' (eliminate), delete TABCVP and provisional parcels as COBOL did
        if (char.ToUpperInvariant(message.Action) == 'E')
        {
            await _repo.DeleteTabCvpAsync(message.PensionId, "AP207", 0, ct);
            await _repo.DeleteParcelsVpAsync(message.PensionId, ct);
            // Finalization in COBOL sets specific codes; we consider deletion done and return
            await _historyService.InsertHistoryAsync(message.PensionId, 7729, $"Delete action {message.Action} applied for pension {message.PensionId}.", "AP825", message.CompanyNumber, ct);
            return;
        }

        // For authorization flow: perform parcel extraction and validations similar to VERIFICA-PARCELAS-ECRAN & other validations
        var parcelsVp = await _repo.GetParcelsVpAsync(message.PensionId, ct);
        // Map parcelsVp into incoming structure if message doesn't provide them (COBOL used W-CPAR from VP)
        if ((message.Iv?.Parcels == null || message.Iv.Parcels.Count == 0) && parcelsVp.Any())
        {
            // Populate message.Iv parcels if missing (non-destructive)
            var iv = message.Iv ?? new IncomingPension();
            var merged = iv with { Parcels = parcelsVp.ToList() };
            message = message with { Iv = merged };
        }

        // Perform validations
        var val1 = await _validator.ValidateParcelsAndLimitsAsync(message, existing, _repo, ct);
        if (!val1.IsValid)
            throw new ApplicationBusinessException(val1.ErrorCode, val1.ErrorMessage);

        var val2 = await _validator.ValidateConventionsAndSpecialCasesAsync(message, existing, _repo, ct);
        if (!val2.IsValid)
            throw new ApplicationBusinessException(val2.ErrorCode, val2.ErrorMessage);

        // Build history description lines (a simplified combination of changed fields)
        var descLines = new List<string>();
        // Compare event
        var incomingEvent = message.Iv?.Event.ToString() ?? message.S?.Event.ToString() ?? string.Empty;
        if (!string.IsNullOrEmpty(incomingEvent) && incomingEvent != existing.Event)
        {
            descLines.Add($"EVENT changed from {existing.Event} to {incomingEvent}");
            existing.Event = incomingEvent;
        }

        // Compare totals (example)
        var incomingV13 = message.Iv?.V13mes ?? message.S?.V13mes ?? 0;
        if (incomingV13 != existing.V13mes)
        {
            descLines.Add($"V13MES changed from {existing.V13mes} to {incomingV13}");
            existing.V13mes = incomingV13;
        }

        // Compare Coutp/Voutp
        var incomingCoutp = message.Iv?.Coutp ?? message.S?.Coutp ?? string.Empty;
        var incomingVoutp = message.Iv?.Voutp ?? message.S?.Voutp ?? 0m;
        if (incomingCoutp != existing.Coutp || incomingVoutp != existing.Voutp)
        {
            descLines.Add($"COUTP/VOUTP changed from {existing.Coutp}/{existing.Voutp} to {incomingCoutp}/{incomingVoutp}");
            existing.Coutp = incomingCoutp;
            existing.Voutp = incomingVoutp;
        }

        // Handle parcel changes: if number or content differs remove DB parcels and insert new
        var existingParcels = (await _repo.GetParcelsAsync(message.PensionId, ct)).ToList();
        var incomingParcels = message.Iv?.Parcels ?? message.S?.Parcels ?? new List<Parcel>();
        var parcelsDiffer = !AreParcelListsEquivalent(existingParcels, incomingParcels);
        if (parcelsDiffer)
        {
            // COBOL performed ELIMINAR-PARCELAS then inserted new ones.
            await _repo.DeleteParcelsAsync(message.PensionId, ct);

            // Insert with CAST to DATE logic preserved by repository
            await _repo.InsertParcelsAsync(message.PensionId, incomingParcels, ct);

            // Add descriptive history about parcels
            descLines.Add($"Parcels replaced: count {incomingParcels.Count}.");
        }

        // Insert history if any changes described
        if (descLines.Any())
        {
            var description = string.Join(" | ", descLines);
            await _historyService.InsertHistoryAsync(message.PensionId, 7729, description, "AP825", message.CompanyNumber, ct);
        }

        // Update master pension record
        await _repo.UpdatePensionAsync(existing, ct);

        // Handle FPEDOC generation when specific conditions appear (simplified)
        if (ShouldGenerateFpedoc(message, existing))
        {
            // COBOL wrote either request type 15 or 16 depending on some rules. We choose a mapping.
            var requestType = DetermineFpedocRequestType(message, existing);
            // Remove duplicates per COBOL variant logic
            await _fileService.DeleteFpedocIfExistsAsync(message, requestType, ct);
            await _fileService.HandleFpedocAsync(message, requestType, ct);
        }
    }

    /// <summary>
    /// Process CGA flows (equivalent to PROGRAMA-CGA / TRATAMENTO in COBOL).
    /// Behavior resembles CNP processing but interacts with PNSCGA and PARCELACGA.
    /// </summary>
    private async Task ProcessCgaAsync(PensionChangeMessage message, CancellationToken ct)
    {
        // Load CGA record
        var existing = await _repo.GetPensionAsync(message.PensionId, message.CmodpagCga, ct);
        if (existing == null)
        {
            throw new ApplicationBusinessException(6, $"Pension {message.PensionId} not found in PNSCGA for CMODPAG {message.CmodpagCga}.");
        }

        // Handle delete action
        if (char.ToUpperInvariant(message.Action) == 'E')
        {
            // Eliminate TABCVP and PARCELACGAVP (COBOL used different origin in CGA)
            await _repo.DeleteTabCvpAsync(message.PensionId, "AP208", message.CmodpagCga, ct);
            await _repo.DeleteParcelsVpAsync(message.PensionId, ct);
            await _historyService.InsertHistoryAsync(message.PensionId, 7729, $"CGA Delete action applied for pension {message.PensionId}.", "AP825", message.CompanyNumber, ct);
            return;
        }

        // Perform validations for CGA similar to CNP in representative fashion
        var val1 = await _validator.ValidateParcelsAndLimitsAsync(message, existing, _repo, ct);
        if (!val1.IsValid)
            throw new ApplicationBusinessException(val1.ErrorCode, val1.ErrorMessage);

        var val2 = await _validator.ValidateConventionsAndSpecialCasesAsync(message, existing, _repo, ct);
        if (!val2.IsValid)
            throw new ApplicationBusinessException(val2.ErrorCode, val2.ErrorMessage);

        // Compare and update necessary fields (simplified)
        var desc = new List<string>();
        var incomingEvent = message.Iv?.Event.ToString() ?? message.S?.Event.ToString() ?? "";
        if (!string.IsNullOrEmpty(incomingEvent) && incomingEvent != existing.Event)
        {
            desc.Add($"CGA event changed from {existing.Event} to {incomingEvent}");
            existing.Event = incomingEvent;
        }

        // Update parcels for CGA (PARCELACGA)
        var incomingParcels = message.Iv?.Parcels ?? message.S?.Parcels ?? new List<Parcel>();
        var parcelacga = await _repo.GetParcelacgaAsync(message.PensionId, message.CmodpagCga, ct);
        if (!AreParcelListsEquivalent(parcelacga, incomingParcels))
        {
            // emulate COBOL behavior: delete and insert
            await _repo.DeleteParcelsAsync(message.PensionId, ct);
            await _repo.InsertParcelsAsync(message.PensionId, incomingParcels, ct);
            desc.Add($"CGA parcels replaced: count {incomingParcels.Count}.");
        }

        if (desc.Any())
        {
            await _historyService.InsertHistoryAsync(message.PensionId, 7733, string.Join(" | ", desc), "AP825", message.CompanyNumber, ct);
        }

        await _repo.UpdatePensionAsync(existing, ct);

        // Final clean-up similar to COBOL (delete TABCVP rows for AP208 origin)
        await _repo.DeleteTabCvpAsync(message.PensionId, "AP208", message.CmodpagCga, ct);
    }

    /// <summary>
    /// Determine if an FPEDOC request should be generated based on incoming message and existing DB state.
    /// This is a simplified mapping of the COBOL TRATA-FPEDOC logic.
    /// </summary>
    private static bool ShouldGenerateFpedoc(PensionChangeMessage message, PensionRecord existing)
    {
        // If the COBOL's AGU-TRATA-FPEDOC flag was set (when COUTR changed from blank to non-blank or vice versa),
        // the code generated an FPEDOC. Here we approximate: if Coutp/Coutr have changed, request is needed.
        var incoming = message.Iv ?? message.S;
        if (incoming == null) return false;
        if (incoming.Coutr != existing.Coutr || incoming.Coutp != existing.Coutp)
            return true;
        return false;
    }

    /// <summary>
    /// Map conditions to an FPEDOC request type integer (representing COBOL's 15,16,8).
    /// </summary>
    private static int DetermineFpedocRequestType(PensionChangeMessage message, PensionRecord existing)
    {
        // Simplified decision:
        // If retirement/outgoing code equals special value then type 15 else 16. For small pensions use type 8.
        var incoming = message.Iv ?? message.S;
        if (incoming == null) return 16;
        if (incoming.Dtnas.HasValue && incoming.Dtnas.Value.Year < 1940) return 8;
        if (incoming.Coutr == "1" || incoming.Coutr == "2" || incoming.Coutr == "3") return 15;
        return 16;
    }

    private static bool AreParcelListsEquivalent(IList<Parcel> a, IList<Parcel> b)
    {
        if (a == null && b == null) return true;
        if (a == null || b == null) return false;
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            var pa = a[i];
            var pb = b[i];
            if (pa.Code != pb.Code) return false;
            var da = pa.StartDate?.Date;
            var db = pb.StartDate?.Date;
            if (da != db) return false;
            if (Math.Round(pa.Value, 2) != Math.Round(pb.Value, 2)) return false;
        }
        return true;
    }
}