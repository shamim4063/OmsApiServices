using MediatR;
using Procurement.Application.Abstractions;

namespace Procurement.Application.Suppliers;

public class GetSupplierByIdHandler : IRequestHandler<GetSupplierById, SupplierDto?>
{
    private readonly ISupplierReader _reader;
    public GetSupplierByIdHandler(ISupplierReader reader) => _reader = reader;
    public Task<SupplierDto?> Handle(GetSupplierById request, CancellationToken cancellationToken)
        => _reader.ById(request.Id, cancellationToken);
}

public class ListSuppliersHandler : IRequestHandler<ListSuppliers, IReadOnlyList<SupplierDto>>
{
    private readonly ISupplierReader _reader;
    public ListSuppliersHandler(ISupplierReader reader) => _reader = reader;
    public Task<IReadOnlyList<SupplierDto>> Handle(ListSuppliers request, CancellationToken cancellationToken)
        => _reader.List(request.Skip, request.Take, cancellationToken);
}

public class CreateSupplierHandler : IRequestHandler<CreateSupplier, Guid>
{
    private readonly ISupplierWriter _writer;
    public CreateSupplierHandler(ISupplierWriter writer) => _writer = writer;
    public Task<Guid> Handle(CreateSupplier cmd, CancellationToken ct)
        => _writer.Add(cmd.Name, cmd.Contact, cmd.TaxId, cmd.IsActive, ct);
}

public class UpdateSupplierHandler : IRequestHandler<UpdateSupplier>
{
    private readonly ISupplierWriter _writer;
    public UpdateSupplierHandler(ISupplierWriter writer) => _writer = writer;
    public Task Handle(UpdateSupplier cmd, CancellationToken ct)
        => _writer.Update(cmd.Id, cmd.Name, cmd.Contact, cmd.TaxId, cmd.IsActive, ct);
}

public class DeleteSupplierHandler : IRequestHandler<DeleteSupplier>
{
    private readonly ISupplierWriter _writer;
    public DeleteSupplierHandler(ISupplierWriter writer) => _writer = writer;
    public Task Handle(DeleteSupplier cmd, CancellationToken ct)
        => _writer.Delete(cmd.Id, ct);
}
