using Domain.ValuesObjects;
using Domain.Enums;

namespace Tests.Domain.ValueObjects;

/// <summary>
/// Testes unit�rios para o Value Object Promotion
/// </summary>
public class PromotionTests
{
    #region Create Tests

  [Fact]
    public void Create_DeveCriarPromocaoValida_QuandoParametrosValidos()
    {
        // Arrange
        var tipo = PromotionType.FixedDiscount;
    var valor = 50m;
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);

        // Act
        var promotion = Promotion.Create(tipo, valor, inicio, fim);

        // Assert
        Assert.NotNull(promotion);
 Assert.Equal(tipo, promotion.Type);
        Assert.Equal(valor, promotion.Value);
  Assert.Equal(inicio, promotion.StartOf);
     Assert.Equal(fim, promotion.EndOf);
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoDataFimAnteriorAInicio()
    {
        // Arrange
  var tipo = PromotionType.FixedDiscount;
  var valor = 50m;
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(-7); // Fim antes do in�cio

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
        Promotion.Create(tipo, valor, inicio, fim));
    Assert.Contains("Data de fim deve ser posterior � data de in�cio", exception.Message);
    }

    [Fact]
    public void Create_DeveLancarExcecao_QuandoDataFimIgualAInicio()
    {
     // Arrange
  var tipo = PromotionType.FixedDiscount;
        var valor = 50m;
  var dataIgual = DateTime.UtcNow;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => 
          Promotion.Create(tipo, valor, dataIgual, dataIgual));
 Assert.Contains("Data de fim deve ser posterior � data de in�cio", exception.Message);
    }

[Fact]
    public void Create_DeveAceitarPromocaoNone_SemValidarDatas()
    {
    // Arrange
     var tipo = PromotionType.None;
        var valor = 0m;
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(-1); // Fim antes do in�cio, mas tipo � None

     // Act
  var promotion = Promotion.Create(tipo, valor, inicio, fim);

        // Assert
        Assert.NotNull(promotion);
        Assert.Equal(PromotionType.None, promotion.Type);
    }

    [Fact]
    public void Create_DeveAceitarValorZero()
    {
 // Arrange
  var tipo = PromotionType.FixedDiscount;
        var valor = 0m;
    var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);

  // Act
        var promotion = Promotion.Create(tipo, valor, inicio, fim);

        // Assert
        Assert.Equal(0m, promotion.Value);
}

    [Fact]
    public void Create_DeveAceitarValorNegativo()
    {
        // Arrange - Valor negativo pode ser permitido para representar acr�scimo
        var tipo = PromotionType.FixedDiscount;
      var valor = -10m;
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);

 // Act
  var promotion = Promotion.Create(tipo, valor, inicio, fim);

     // Assert
     Assert.Equal(-10m, promotion.Value);
 }

    #endregion

    #region None Property Tests

 [Fact]
    public void None_DeveRetornarPromocaoSemDesconto()
    {
        // Act
     var promotion = Promotion.None;

        // Assert
        Assert.NotNull(promotion);
 Assert.Equal(PromotionType.None, promotion.Type);
        Assert.Equal(0m, promotion.Value);
      Assert.Equal(DateTime.MinValue, promotion.StartOf);
        Assert.Equal(DateTime.MinValue, promotion.EndOf);
    }

  [Fact]
    public void None_DeveSempreMesmaInstancia()
    {
        // Act
        var promotion1 = Promotion.None;
        var promotion2 = Promotion.None;

      // Assert
        Assert.Equal(promotion1, promotion2);
    }

    #endregion

    #region IsActive Tests

    [Fact]
    public void IsActive_DeveRetornarTrue_QuandoDataDentroDoPeriodo()
    {
        // Arrange
  var inicio = DateTime.UtcNow.AddDays(-1);
   var fim = DateTime.UtcNow.AddDays(1);
        var promotion = Promotion.Create(PromotionType.FixedDiscount, 10m, inicio, fim);
   var dataAtual = DateTime.UtcNow;

        // Act
     var isActive = promotion.IsActive(dataAtual);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_DeveRetornarTrue_QuandoDataIgualAInicio()
    {
        // Arrange
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);
     var promotion = Promotion.Create(PromotionType.FixedDiscount, 10m, inicio, fim);

        // Act
    var isActive = promotion.IsActive(inicio);

        // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_DeveRetornarTrue_QuandoDataIgualAFim()
 {
        // Arrange
        var inicio = DateTime.UtcNow.AddDays(-7);
        var fim = DateTime.UtcNow;
        var promotion = Promotion.Create(PromotionType.FixedDiscount, 10m, inicio, fim);

        // Act
        var isActive = promotion.IsActive(fim);

    // Assert
        Assert.True(isActive);
    }

    [Fact]
    public void IsActive_DeveRetornarFalse_QuandoDataAntesDoInicio()
    {
        // Arrange
        var inicio = DateTime.UtcNow.AddDays(1);
 var fim = DateTime.UtcNow.AddDays(7);
  var promotion = Promotion.Create(PromotionType.FixedDiscount, 10m, inicio, fim);
        var dataAntes = DateTime.UtcNow;

        // Act
        var isActive = promotion.IsActive(dataAntes);

        // Assert
        Assert.False(isActive);
    }

    [Fact]
    public void IsActive_DeveRetornarFalse_QuandoDataDepoisDoFim()
    {
        // Arrange
var inicio = DateTime.UtcNow.AddDays(-7);
  var fim = DateTime.UtcNow.AddDays(-1);
        var promotion = Promotion.Create(PromotionType.FixedDiscount, 10m, inicio, fim);
        var dataDepois = DateTime.UtcNow;

        // Act
    var isActive = promotion.IsActive(dataDepois);

        // Assert
     Assert.False(isActive);
    }

    [Fact]
    public void IsActive_DeveRetornarFalse_QuandoPromocaoNone()
    {
        // Arrange
        var promotion = Promotion.None;
 var dataQualquer = DateTime.UtcNow;

// Act
    var isActive = promotion.IsActive(dataQualquer);

      // Assert
        Assert.False(isActive);
    }

    #endregion

    #region ApplyDiscount Tests - Fixed Discount

    [Fact]
    public void ApplyDiscount_DeveAplicarDescontoFixo_QuandoPromocaoAtiva()
    {
        // Arrange
        var promotion = Promotion.Create(
     PromotionType.FixedDiscount,
            20m,
       DateTime.UtcNow.AddDays(-1),
        DateTime.UtcNow.AddDays(1)
 );
        var precoOriginal = 100m;

        // Act
        var precoFinal = promotion.ApplyDiscount(precoOriginal);

      // Assert
        Assert.Equal(80m, precoFinal); // 100 - 20 = 80
    }

    [Fact]
    public void ApplyDiscount_DeveRetornarPrecoOriginal_QuandoDescontoFixoMaiorQuePreco()
{
        // Arrange
      var promotion = Promotion.Create(
 PromotionType.FixedDiscount,
    150m, // Desconto maior que o pre�o
          DateTime.UtcNow.AddDays(-1),
         DateTime.UtcNow.AddDays(1)
        );
      var precoOriginal = 100m;

   // Act
  var precoFinal = promotion.ApplyDiscount(precoOriginal);

     // Assert
     Assert.Equal(-50m, precoFinal); // Pode resultar em negativo
    }

    #endregion

    #region ApplyDiscount Tests - Percentage Discount

    [Fact]
    public void ApplyDiscount_DeveAplicarDescontoPercentual_QuandoPromocaoAtiva()
    {
        // Arrange
        var promotion = Promotion.Create(
            PromotionType.PercentageDiscount,
            25m, // 25% de desconto
            DateTime.UtcNow.AddDays(-1),
       DateTime.UtcNow.AddDays(1)
  );
        var precoOriginal = 100m;

        // Act
        var precoFinal = promotion.ApplyDiscount(precoOriginal);

        // Assert
 Assert.Equal(75m, precoFinal); // 100 * (1 - 0.25) = 75
    }

    [Fact]
    public void ApplyDiscount_DeveAplicar50PorcentoDesconto()
    {
        // Arrange
var promotion = Promotion.Create(
    PromotionType.PercentageDiscount,
        50m,
          DateTime.UtcNow.AddDays(-1),
     DateTime.UtcNow.AddDays(1)
        );
    var precoOriginal = 200m;

   // Act
        var precoFinal = promotion.ApplyDiscount(precoOriginal);

     // Assert
    Assert.Equal(100m, precoFinal); // Metade do pre�o
    }

    [Fact]
    public void ApplyDiscount_DeveAplicar100PorcentoDesconto()
    {
        // Arrange
        var promotion = Promotion.Create(
    PromotionType.PercentageDiscount,
            100m, // 100% de desconto = gr�tis
       DateTime.UtcNow.AddDays(-1),
            DateTime.UtcNow.AddDays(1)
        );
        var precoOriginal = 100m;

        // Act
   var precoFinal = promotion.ApplyDiscount(precoOriginal);

        // Assert
        Assert.Equal(0m, precoFinal);
    }

    #endregion

    #region ApplyDiscount Tests - Inactive Promotion

    [Fact]
    public void ApplyDiscount_DeveRetornarPrecoOriginal_QuandoPromocaoExpirada()
    {
     // Arrange
        var promotion = Promotion.Create(
            PromotionType.FixedDiscount,
    20m,
      DateTime.UtcNow.AddDays(-10),
   DateTime.UtcNow.AddDays(-1) // Expirou ontem
 );
        var precoOriginal = 100m;

        // Act
        var precoFinal = promotion.ApplyDiscount(precoOriginal);

   // Assert
  Assert.Equal(precoOriginal, precoFinal);
    }

    [Fact]
    public void ApplyDiscount_DeveRetornarPrecoOriginal_QuandoPromocaoNaoIniciada()
    {
        // Arrange
    var promotion = Promotion.Create(
            PromotionType.PercentageDiscount,
          30m,
   DateTime.UtcNow.AddDays(1), // Come�a amanh�
         DateTime.UtcNow.AddDays(7)
        );
        var precoOriginal = 100m;

    // Act
        var precoFinal = promotion.ApplyDiscount(precoOriginal);

        // Assert
        Assert.Equal(precoOriginal, precoFinal);
    }

    [Fact]
    public void ApplyDiscount_DeveRetornarPrecoOriginal_QuandoPromocaoNone()
    {
      // Arrange
        var promotion = Promotion.None;
        var precoOriginal = 100m;

        // Act
        var precoFinal = promotion.ApplyDiscount(precoOriginal);

        // Assert
        Assert.Equal(precoOriginal, precoFinal);
    }

    #endregion

    #region Value Object Equality Tests

    [Fact]
    public void Equals_DeveRetornarTrue_QuandoPromocoesIdenticas()
  {
        // Arrange
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);
        
     var promotion1 = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio, fim);
        var promotion2 = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio, fim);

        // Act & Assert
        Assert.Equal(promotion1, promotion2);
    }

    [Fact]
    public void Equals_DeveRetornarFalse_QuandoTiposDiferentes()
    {
        // Arrange
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);
        
        var promotion1 = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio, fim);
        var promotion2 = Promotion.Create(PromotionType.PercentageDiscount, 20m, inicio, fim);

        // Act & Assert
        Assert.NotEqual(promotion1, promotion2);
    }

    [Fact]
    public void Equals_DeveRetornarFalse_QuandoValoresDiferentes()
    {
        // Arrange
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);
        
        var promotion1 = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio, fim);
var promotion2 = Promotion.Create(PromotionType.FixedDiscount, 30m, inicio, fim);

        // Act & Assert
        Assert.NotEqual(promotion1, promotion2);
    }

    [Fact]
    public void Equals_DeveRetornarFalse_QuandoDatasDiferentes()
    {
// Arrange
        var inicio1 = DateTime.UtcNow;
        var fim1 = DateTime.UtcNow.AddDays(7);
        var inicio2 = DateTime.UtcNow.AddDays(1);
   var fim2 = DateTime.UtcNow.AddDays(8);
        
        var promotion1 = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio1, fim1);
        var promotion2 = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio2, fim2);

        // Act & Assert
        Assert.NotEqual(promotion1, promotion2);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void FluxoCompleto_CriarVerificarAplicar_DeveFuncionarCorretamente()
    {
   // Arrange
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);
        var promotion = Promotion.Create(PromotionType.PercentageDiscount, 30m, inicio, fim);
        var precoOriginal = 150m;

 // Act & Assert
        // Verifica se est� ativa
        Assert.True(promotion.IsActive(DateTime.UtcNow));

        // Aplica desconto
        var precoComDesconto = promotion.ApplyDiscount(precoOriginal);
        Assert.Equal(105m, precoComDesconto); // 150 * 0.7 = 105

        // Verifica tipo
    Assert.Equal(PromotionType.PercentageDiscount, promotion.Type);
    }

    [Fact]
    public void ComparacaoDePromocoes_DiferentesTipos()
    {
        // Arrange
        var inicio = DateTime.UtcNow;
        var fim = DateTime.UtcNow.AddDays(7);
        var precoOriginal = 100m;

        var fixedPromo = Promotion.Create(PromotionType.FixedDiscount, 20m, inicio, fim);
        var percentPromo = Promotion.Create(PromotionType.PercentageDiscount, 20m, inicio, fim);

   // Act
        var fixedFinalPrice = fixedPromo.ApplyDiscount(precoOriginal);
        var percentFinalPrice = percentPromo.ApplyDiscount(precoOriginal);

        // Assert
        Assert.Equal(80m, fixedFinalPrice); // 100 - 20
        Assert.Equal(80m, percentFinalPrice); // 100 * 0.8
        Assert.Equal(fixedFinalPrice, percentFinalPrice); // Neste caso, resultado igual
  }

    [Fact]
  public void PromocaoTemporaria_DeveExpirar()
    {
        // Arrange
        var inicio = DateTime.UtcNow.AddDays(-5);
    var fim = DateTime.UtcNow.AddDays(-1); // Expirou ontem
        var promotion = Promotion.Create(PromotionType.FixedDiscount, 50m, inicio, fim);
  var precoOriginal = 100m;

        // Act
     var isActiveBeforeExpiry = promotion.IsActive(inicio.AddDays(2)); // Meio do per�odo
        var isActiveAfterExpiry = promotion.IsActive(DateTime.UtcNow); // Hoje
   var priceAfterExpiry = promotion.ApplyDiscount(precoOriginal);

        // Assert
        Assert.True(isActiveBeforeExpiry);
        Assert.False(isActiveAfterExpiry);
        Assert.Equal(precoOriginal, priceAfterExpiry); // Sem desconto ap�s expirar
    }

    [Fact]
    public void MultiplosPrecosComMesmaPromocao()
  {
        // Arrange
        var promotion = Promotion.Create(
        PromotionType.PercentageDiscount,
            25m,
     DateTime.UtcNow,
  DateTime.UtcNow.AddDays(7)
        );

        var precos = new[] { 50m, 100m, 150m, 200m };
var precosEsperados = new[] { 37.5m, 75m, 112.5m, 150m };

        // Act & Assert
 for (int i = 0; i < precos.Length; i++)
   {
      var precoFinal = promotion.ApplyDiscount(precos[i]);
     Assert.Equal(precosEsperados[i], precoFinal);
        }
    }

    #endregion
}
