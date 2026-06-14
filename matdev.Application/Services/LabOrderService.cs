using matdev.Application.DTOs.LabOrder;

using matdev.Application.Interfaces;

using matdev.Domain.Entities.LabEntities;

using matdev.Domain.Interfaces;



namespace matdev.Application.Services;



public class LabOrderService : ILabOrderService

{

    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)

    {

        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".csv", ".png", ".jpg", ".jpeg",

    };



    private readonly ILabOrderRepository _repository;

    private readonly IProjectRepository _projectRepository;

    private readonly ILabOrderFileStorage _fileStorage;



    public LabOrderService(

        ILabOrderRepository repository,

        IProjectRepository projectRepository,

        ILabOrderFileStorage fileStorage)

    {

        _repository = repository;

        _projectRepository = projectRepository;

        _fileStorage = fileStorage;

    }



    public async Task<IReadOnlyList<GetLabOrderDTO>> GetByProjectAsync(int projectId)

    {

        await EnsureProjectExists(projectId);

        var orders = await _repository.GetByProjectAsync(projectId);

        return orders.Select(MapToDto).ToList();

    }



    public async Task<IReadOnlyList<GetLabOrderStatusDTO>> GetStatusesAsync()

    {

        var statuses = await _repository.GetStatusesAsync();

        return statuses.Select(s => new GetLabOrderStatusDTO(s.LabOrderStatusID, s.Name)).ToList();

    }



    public async Task<GetLabOrderDTO> CreateAsync(int projectId, CreateLabOrderDTO dto)

    {

        await EnsureProjectExists(projectId);



        var description = dto.Description?.Trim() ?? "";

        if (string.IsNullOrEmpty(description))

            throw new ArgumentException("Description is required.");



        LabOrderStatus? status = null;

        if (dto.StatusId is int statusId)

        {

            status = await _repository.GetStatusByIdAsync(statusId)

                ?? throw new KeyNotFoundException($"Lab order status {statusId} not found.");

        }

        else

        {

            var statuses = await _repository.GetStatusesAsync();

            status = statuses.FirstOrDefault(s => s.Name == "Created")

                ?? statuses.FirstOrDefault();

        }



        var order = new LabOrder

        {

            Description = description,

            SampleID = NormalizeOptional(dto.SampleId),

            StatusID = status?.LabOrderStatusID,

            Status = status!,

            CreatedAt = DateTime.UtcNow,

            PlannedCompletionDate = ToUtcDate(dto.PlannedCompletionDate),

            PredictedCompletionDate = ToUtcDate(dto.PredictedCompletionDate),

        };



        var created = await _repository.AddAsync(order, projectId);

        created.Status = status!;

        return MapToDto(created);

    }



    public async Task<GetLabOrderDTO> UpdateAsync(int projectId, int labOrderId, UpdateLabOrderDTO dto)

    {

        var order = await GetOrderOrThrow(projectId, labOrderId);



        if (dto.Description is not null)

        {

            var description = dto.Description.Trim();

            if (string.IsNullOrEmpty(description))

                throw new ArgumentException("Description cannot be empty.");

            order.Description = description;

        }



        if (dto.SampleId is not null)

            order.SampleID = NormalizeOptional(dto.SampleId);



        if (dto.StatusId is int statusId)

        {

            var status = await _repository.GetStatusByIdAsync(statusId)

                ?? throw new KeyNotFoundException($"Lab order status {statusId} not found.");

            order.StatusID = status.LabOrderStatusID;

            order.Status = status;



            if (status.Name.Equals("Completed", StringComparison.OrdinalIgnoreCase)

                && order.CompletionDate is null)

            {

                order.CompletionDate = DateTime.UtcNow;

            }

        }



        if (dto.PlannedCompletionDate.HasValue)

            order.PlannedCompletionDate = ToUtcDate(dto.PlannedCompletionDate);



        if (dto.PredictedCompletionDate.HasValue)

            order.PredictedCompletionDate = ToUtcDate(dto.PredictedCompletionDate);



        if (dto.CompletionDate.HasValue)

            order.CompletionDate = ToUtcDate(dto.CompletionDate);



        if (dto.TestReportFileName is not null)

            order.TestReportFileName = NormalizeOptional(dto.TestReportFileName);



        if (dto.TestReportLink is not null)

        {

            if (ILabOrderFileStorage.IsStoredLink(order.TestReportLink))

                throw new InvalidOperationException("Use report upload endpoints for stored files.");

            order.TestReportLink = NormalizeOptional(dto.TestReportLink);

        }



        if (dto.FinalReportFileName is not null)

            order.FinalReportFileName = NormalizeOptional(dto.FinalReportFileName);



        if (dto.FinalReportLink is not null)

        {

            if (ILabOrderFileStorage.IsStoredLink(order.FinalReportLink))

                throw new InvalidOperationException("Use report upload endpoints for stored files.");

            order.FinalReportLink = NormalizeOptional(dto.FinalReportLink);

        }



        await _repository.UpdateAsync(order);

        return MapToDto(order);

    }



    public async Task DeleteAsync(int projectId, int labOrderId)

    {

        var order = await GetOrderOrThrow(projectId, labOrderId);

        _fileStorage.DeleteOrderFolder(projectId, labOrderId);

        await _repository.DeleteAsync(order);

    }



    public async Task<GetLabOrderDTO> UploadTestReportAsync(int projectId, int labOrderId, Stream content, string fileName)

    {

        ValidateFileName(fileName);

        var order = await GetOrderOrThrow(projectId, labOrderId);

        _fileStorage.DeleteIfExists(order.TestReportLink);



        var storedLink = await _fileStorage.SaveAsync(projectId, labOrderId, "test-report", content, fileName);

        order.TestReportFileName = Path.GetFileName(fileName);

        order.TestReportLink = storedLink;

        await _repository.UpdateAsync(order);

        return MapToDto(order);

    }



    public async Task<GetLabOrderDTO> UploadFinalReportAsync(int projectId, int labOrderId, Stream content, string fileName)

    {

        ValidateFileName(fileName);

        var order = await GetOrderOrThrow(projectId, labOrderId);

        _fileStorage.DeleteIfExists(order.FinalReportLink);



        var storedLink = await _fileStorage.SaveAsync(projectId, labOrderId, "final-report", content, fileName);

        order.FinalReportFileName = Path.GetFileName(fileName);

        order.FinalReportLink = storedLink;

        await _repository.UpdateAsync(order);

        return MapToDto(order);

    }



    public async Task<(Stream Stream, string FileName, string ContentType)?> GetTestReportFileAsync(int projectId, int labOrderId)

    {

        var order = await GetOrderOrThrow(projectId, labOrderId);

        return _fileStorage.OpenRead(order.TestReportLink, order.TestReportFileName);

    }



    public async Task<(Stream Stream, string FileName, string ContentType)?> GetFinalReportFileAsync(int projectId, int labOrderId)

    {

        var order = await GetOrderOrThrow(projectId, labOrderId);

        return _fileStorage.OpenRead(order.FinalReportLink, order.FinalReportFileName);

    }



    public async Task<GetLabOrderDTO> DeleteTestReportAsync(int projectId, int labOrderId)

    {

        var order = await GetOrderOrThrow(projectId, labOrderId);

        _fileStorage.DeleteIfExists(order.TestReportLink);

        order.TestReportFileName = null;

        order.TestReportLink = null;

        await _repository.UpdateAsync(order);

        return MapToDto(order);

    }



    public async Task<GetLabOrderDTO> DeleteFinalReportAsync(int projectId, int labOrderId)

    {

        var order = await GetOrderOrThrow(projectId, labOrderId);

        _fileStorage.DeleteIfExists(order.FinalReportLink);

        order.FinalReportFileName = null;

        order.FinalReportLink = null;

        await _repository.UpdateAsync(order);

        return MapToDto(order);

    }



    private async Task<LabOrder> GetOrderOrThrow(int projectId, int labOrderId)

    {

        return await _repository.GetByIdForProjectAsync(projectId, labOrderId)

            ?? throw new KeyNotFoundException($"Lab order {labOrderId} not found in project {projectId}.");

    }



    private async Task EnsureProjectExists(int projectId)

    {

        var project = await _projectRepository.GetByIdAsync(projectId);

        if (project is null)

            throw new KeyNotFoundException($"Project {projectId} not found.");

    }



    private static void ValidateFileName(string fileName)

    {

        var ext = Path.GetExtension(fileName);

        if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))

            throw new ArgumentException($"File type not allowed. Allowed: {string.Join(", ", AllowedExtensions)}");

    }



    private static string? NormalizeOptional(string? value)

    {

        if (value is null) return null;

        var trimmed = value.Trim();

        return string.IsNullOrEmpty(trimmed) ? null : trimmed;

    }



    private static DateTime? ToUtcDate(DateTime? value)

    {

        if (value is null) return null;

        return DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Utc);

    }



    private static GetLabOrderDTO MapToDto(LabOrder o) =>

        new(

            o.LabOrderID,

            o.Description,

            o.SampleID,

            o.StatusID,

            o.Status?.Name,

            o.CreatedAt,

            o.PlannedCompletionDate,

            o.PredictedCompletionDate,

            o.CompletionDate,

            o.TestReportFileName,

            o.TestReportLink,

            o.FinalReportFileName,

            o.FinalReportLink,

            ILabOrderFileStorage.IsStoredLink(o.TestReportLink),

            ILabOrderFileStorage.IsStoredLink(o.FinalReportLink));

}

