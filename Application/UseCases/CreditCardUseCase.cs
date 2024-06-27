using System.Text.Json.Serialization;
using System.Linq;

namespace Application.UseCases;

public static class CreditCardUseCase
{
    public static CreditCardRateCalculatorResponse CreditCardRateCalculator(decimal desiredValue, string flag)
    {
        if (desiredValue <= 0) 
            return null;
        
        var amountToBeCharged = ResolveAmountsToBeCharged(desiredValue, flag);
        return amountToBeCharged;
    }

    #region private metods

    private static CreditCardRateCalculatorResponse ResolveAmountsToBeCharged(decimal desiredValue, string flag)
    {
        var tempLiquidValues = ResolveFinalLiquidValues(desiredValue, flag);
        var response = new CreditCardRateCalculatorResponse(
            Math.Round(BillingAmountCalculator(desiredValue, tempLiquidValues.P1), 2),
            0,
            0,
            0,
            0,
            Math.Round(BillingAmountCalculator(desiredValue, tempLiquidValues.P6.Sum()), 2),
            0,
            0,
            0,
            0,
            0,
            Math.Round(BillingAmountCalculator(desiredValue, tempLiquidValues.P12.Take(12).Sum()),2));
        return response;
    }

    private static OneSixTwelveParcels ResolveFinalLiquidValues(decimal desiredValue, string flag)
    {
        var partialLiquidValues = ResolvePartialLiquidValues(desiredValue, flag);
        var P1FinalLiquidValues = ApplyAnticipationDiscounts(new List<decimal>{partialLiquidValues.P1}, flag)[0];
        var P6FinalLiquidValues = ApplyAnticipationDiscounts(partialLiquidValues.P6, flag);
        var P12FinalLiquidValues = ApplyAnticipationDiscounts(partialLiquidValues.P12, flag);
        return new OneSixTwelveParcels(P1FinalLiquidValues, P6FinalLiquidValues, P12FinalLiquidValues);
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
            var AnticipationDiscount = effectiveAnticipationTax / 100 * partialLiquidValues[i];
            partialLiquidValues[i] -= AnticipationDiscount;
        }

        return partialLiquidValues;
    }

    private static OneSixTwelveParcels ResolvePartialLiquidValues(decimal desiredValue, string flag)
    {
        var oneParcel = ApplyMdrDiscount(new List<decimal>{desiredValue}, flag)[0];
        var sixParcels = ApplyMdrDiscount(ResolveParcels(desiredValue, 6), flag);
        var twelveParcels = ApplyMdrDiscount(ResolveParcels(desiredValue, 12), flag);
        return new OneSixTwelveParcels(oneParcel, sixParcels, twelveParcels);
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
        
        for (var i = 0; i < parcels.Count; i++)
        {
            var taxMdr = parcels.Count switch
            {
                1 => taxesMdr.P1,
                <= 6 and > 1 => taxesMdr.P2P6,
                _ => taxesMdr.P7P12
            };
            
            parcels[i] -= taxMdr / 100 * parcels[i];
        }

        return parcels;
    }

    private static List<decimal> ResolveParcels(decimal desiredValue, int numberOfParcels)
    {
        var valuePerParcel = Math.Round(desiredValue / numberOfParcels, 2);
        var sumOfParcels = valuePerParcel * numberOfParcels;
        
        var parcelsList = new List<decimal>();
        for (int i = 0; i < numberOfParcels; i++)
        {
            parcelsList.Add(valuePerParcel);
        }
        
        // Ajusta o último valor para garantir que a soma seja igual ao valor total
        parcelsList[numberOfParcels - 1] += desiredValue - sumOfParcels;

        return parcelsList;
    }

    private static decimal BillingAmountCalculator(decimal desiredValue, decimal billingAmount)
    {
        return (desiredValue * desiredValue) / billingAmount;
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

    private record OneSixTwelveParcels(decimal P1, List<decimal> P6, List<decimal> P12);

    #endregion

}