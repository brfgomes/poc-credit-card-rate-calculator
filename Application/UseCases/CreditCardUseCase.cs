using System.Text.Json.Serialization;

namespace Application.UseCases;

public static class CreditCardUseCase
{
    public static CreditCardRateCalculatorResponse CreditCardRateCalculator(decimal desiredValue, string flag)
    {
        if (desiredValue <= 0)
            return null;
        
        var amountToBeCharged = ResolveAmountsToBeCharged(desiredValue, flag);
        var response = new CreditCardRateCalculatorResponse(
            amountToBeCharged[0],
            amountToBeCharged[1],
            amountToBeCharged[2],
            amountToBeCharged[3],
            amountToBeCharged[4],
            amountToBeCharged[5],
            amountToBeCharged[6],
            amountToBeCharged[7],
            amountToBeCharged[8],
            amountToBeCharged[9],
            amountToBeCharged[10],
            amountToBeCharged[11]);
        
        return response;
    }

    #region private metods

    private static List<decimal> ResolveAmountsToBeCharged(decimal desiredValue, string flag)
    {
        var finalLiquidValues = ResolveFinalLiquidValues(desiredValue, flag);
        var totalAmountToBeCharged = finalLiquidValues.Sum();
        var amountsToBeCharged = ResolveFinalLiquidValues(totalAmountToBeCharged, flag);
        return amountsToBeCharged;
    }

    private static List<decimal> ResolveFinalLiquidValues(decimal desiredValue, string flag)
    {
        var partialLiquidValues = ResolvePartialLiquidValues(desiredValue, flag);
        var finalLiquidValues = ApplyAnticipationDiscounts(partialLiquidValues, flag);
        return finalLiquidValues;
    }

    private static List<decimal> ApplyAnticipationDiscounts(List<decimal> partialLiquidValues, string flag)
    {
        var fixedAnticipationTax = 0m;

        //adicionar taxas de outros cartões nesse switch quando tiver as taxas
        switch (flag)
        {
            default:
                fixedAnticipationTax = 1.09m;
                break;
        }

        for (int i = 0; i < partialLiquidValues.Count; i++)
        {
            var parcelNumber = i + 1;
            var effectiveAnticipationTax = fixedAnticipationTax * parcelNumber;
            var AnticipationDiscount = partialLiquidValues[i] * effectiveAnticipationTax;
            partialLiquidValues[i] -= AnticipationDiscount;
        }

        return partialLiquidValues;
    }

    private static List<decimal> ResolvePartialLiquidValues(decimal desiredValue, string flag)
    {
        var parcels = ResolveParcels(desiredValue);
        return ApplyMdrDiscount(parcels, flag);
    }

    private static List<decimal> ApplyMdrDiscount(List<decimal> parcels, string flag)
    {
        var taxesMdr = new TaxMdr(0,0,0);
        
        //adicionar taxas de outros cartões nesse switch quando tiver as taxas
        switch (flag)
        {
            default:
                taxesMdr = new TaxMdr(0.9m, 2.40m, 2.50m);
                break;
        }
        
        for (int i = 0; i < parcels.Count; i++)
        {
            decimal taxMdr = 0m;

            switch (i)
            {
                case 0:
                    taxMdr = taxesMdr.P1;
                    break;
                case int n when (n >= 1 && n <= 5):
                    taxMdr = taxesMdr.P2P6;
                    break;
                default:
                    taxMdr = taxesMdr.P7P12;
                    break;
            }

            parcels[i] -= taxMdr;
        }

        return parcels;
    }

    private static List<decimal> ResolveParcels(decimal desiredValue)
    {
        var maxNumberOfParcels = 12;
        var valuePerParcel = Math.Round(desiredValue / maxNumberOfParcels, 2);
        var sumOfParcels = valuePerParcel * maxNumberOfParcels;
        
        var parcelsList = new List<decimal>();
        for (int i = 0; i < maxNumberOfParcels; i++)
        {
            parcelsList.Add(valuePerParcel);
        }
        
        // Ajusta o último valor para garantir que a soma seja igual ao valor total
        parcelsList[maxNumberOfParcels - 1] += desiredValue - sumOfParcels;

        return parcelsList;
    }

    #endregion

    #region records

    public record CreditCardRateCalculatorResponse(
        decimal X1,
        decimal X2,
        decimal X3,
        decimal X4,
        decimal X5,
        decimal X6,
        decimal X7,
        decimal X8,
        decimal X9,
        decimal X10,
        decimal X11,
        decimal X12);
    private record TaxMdr(decimal P1, decimal P2P6, decimal P7P12);
        
    #endregion

}