using Application.Dto.Filter;
using Application.Dto.Order;
using Application.Dto.Request;
using Application.Dto.Response;
using Application.Dto.Result;
using Microsoft.AspNetCore.Mvc;

namespace Application.Interfaces.Service;

public interface IPurchaseService
{
    Task<OperationResult<PurchaseResponseDto>> RegisterPurchaseAsync(PurchaseCreateRequestDto dto);
    Task<OperationResult> RegisterPurchaseAsync(Guid playerId);
    Task<OperationResult<PagedResult<PurchaseResponseDto>>> GetPurchasesByPlayerAsync(Guid playerId, PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto);
    Task<OperationResult<IEnumerable<PurchaseResponseDto>>> GetAllPurchasesAsync();
    Task<OperationResult<PagedResult<PurchaseResponseDto>>> GetAllPurchasesAsync(PagedRequestDto<PurchaseFilterDto?, PurchaseOrderDto?> dto);
    
    /// <summary>
    /// Gets purchase statistics for the specified period or all time if no dates provided
    /// </summary>
    /// <param name="startDate">Optional start date for the period</param>
    /// <param name="endDate">Optional end date for the period</param>
    /// <returns>Purchase statistics including total purchases, revenue, and period details</returns>
    Task<OperationResult<PurchaseStatsDto>> GetPurchaseStatsAsync(DateTime? startDate = null, DateTime? endDate = null);
}