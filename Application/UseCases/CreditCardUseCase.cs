namespace Application.UseCases;

public static class CreditCardUseCase
{
    public static CreditCardRateCalculatorResponse CreditCardRateCalculator(decimal desiredValue, string flag)
    {
        if (desiredValue <= 0) { return null; }
        return ResolveAmountsToBeCharged(desiredValue, flag);
    }

    #region private metods

    private static CreditCardRateCalculatorResponse ResolveAmountsToBeCharged(decimal desiredValue, string flag)
    {
        var finalLiquidValues = ResolveFinalLiquidValues(desiredValue, flag);
        var response = new CreditCardRateCalculatorResponse(
            BillingAmountCalculator(desiredValue, finalLiquidValues.P1),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P2.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P3.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P4.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P5.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P6.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P7.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P8.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P9.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P10.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P11.Sum()),
            BillingAmountCalculator(desiredValue, finalLiquidValues.P12.Sum()));
        return response;
    }

    private static Parcels ResolveFinalLiquidValues(decimal desiredValue, string flag)
    {
        var partialLiquidValues = ResolvePartialLiquidValues(desiredValue, flag);
        var response = new Parcels(
            ApplyAnticipationDiscounts(new List<decimal>{partialLiquidValues.P1}, flag)[0],
            ApplyAnticipationDiscounts(partialLiquidValues.P2, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P3, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P4, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P5, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P6, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P7, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P8, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P9, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P10, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P11, flag),
            ApplyAnticipationDiscounts(partialLiquidValues.P12, flag));
        return response;
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
            var anticipationDiscount = effectiveAnticipationTax / 100 * partialLiquidValues[i];
            partialLiquidValues[i] -= anticipationDiscount;
        }

        return partialLiquidValues;
    }

    private static Parcels ResolvePartialLiquidValues(decimal desiredValue, string flag)
    {
        var desiredValueList = new List<decimal> { desiredValue };
        var parcelValues = new List<List<decimal>>();
        var p1 = ApplyMdrDiscount(desiredValueList, flag)[0];
        
        for (int i = 2; i <= 12; i++)
        {
            parcelValues.Add(ApplyMdrDiscount(ResolveParcels(desiredValue, i), flag));
        }
        
        return new Parcels(
            p1,
            parcelValues[0],
            parcelValues[1],
            parcelValues[2],
            parcelValues[3],
            parcelValues[4],
            parcelValues[5],
            parcelValues[6],
            parcelValues[7],
            parcelValues[8],
            parcelValues[9],
            parcelValues[10]
        );
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
        return Math.Round((desiredValue * desiredValue) / billingAmount, 2);
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

    private record Parcels(
        decimal P1,
        List<decimal> P2,
        List<decimal> P3,
        List<decimal> P4,
        List<decimal> P5,
        List<decimal> P6,
        List<decimal> P7,
        List<decimal> P8,
        List<decimal> P9,
        List<decimal> P10,
        List<decimal> P11,
        List<decimal> P12);

    #endregion

}