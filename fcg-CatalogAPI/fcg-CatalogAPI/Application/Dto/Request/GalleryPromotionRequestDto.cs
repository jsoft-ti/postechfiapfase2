using System;

namespace Application.Dto.Request;

/// <summary>
/// DTO for applying promotions to gallery games
/// </summary>
public class GalleryPromotionRequestDto
{
    /// <summary>
    /// Type of promotion to apply
    /// </summary>
    public string PromotionType { get; set; } = string.Empty;

    /// <summary>
    /// Value of the promotion (e.g., discount percentage or fixed amount)
    /// </summary>
    public decimal PromotionValue { get; set; }

    /// <summary>
    /// When the promotion starts (optional)
    /// </summary>
    public DateTime? PromotionStartDate { get; set; }

    /// <summary>
    /// When the promotion ends (optional)
    /// </summary>
    public DateTime? PromotionEndDate { get; set; }
}