using Xunit;
using GestionProduccion.Resources;

namespace GestionProduccion.Tests.Integration;

public class LocalizationTests
{
    [Fact]
    public void PortugueseResources_ShouldNotContainSpanishWords()
    {
        // This test checks for common "PortuÃ±ol" mistakes identified in the 
        // inconsistencies report.
        
        // Error: "es" (Spanish) vs "Ã©" (Portuguese)
        Assert.DoesNotContain(" es ", Portuguese.Prod_ErrNameRequired);
        
        // Error: "no" (Spanish) vs "nÃ£o" (Portuguese)
        Assert.DoesNotContain(" no ", Portuguese.Prod_ErrSkuLength);
        
        // Error: "con" (Spanish) vs "com" (Portuguese)
        Assert.DoesNotContain(" con ", Portuguese.OP_OrderCreatedSuccess);
    }
}
