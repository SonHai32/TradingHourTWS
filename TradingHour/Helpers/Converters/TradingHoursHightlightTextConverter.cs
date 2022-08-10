using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using TradingHour.Models;

namespace TradingHour.Helpers.Converters
{
    public enum HightLightColor
    {
        RED, GREEN, WHITE
    }

    public class TradingHoursHightlightTextConverter : IValueConverter
    {
        private BrushConverter _brushConverter = new BrushConverter();

        private TextBlock DefaultTextBlock => new TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(4),
            FontSize = 14,
            FontWeight = FontWeights.Medium
        };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var textBlock = DefaultTextBlock;
            textBlock.TextAlignment = TextAlignment.Center;

            if (value == null)
            {
                textBlock.Inlines.Add(GetTextHightLightColor(StaticConstants.NO_TRADING_HOUR_INFO, HightLightColor.RED));
                return textBlock;
            }
            if (value is List<TradingHours> tradingHours)
            {
                if (!tradingHours.Any())
                    return NoInfoTrading();
                if (tradingHours.Count < 2)
                {
                    if (tradingHours.FirstOrDefault().IsTrandingClosed)
                        return ClosingTrading();
                    var tradingHour = tradingHours.FirstOrDefault();
                    textBlock.Inlines.Add(GetTextHightLightColor(tradingHour.OpeningTime.ToString("HH:mm"), HightLightColor.GREEN));
                    textBlock.Inlines.Add(GetTextHightLightColor("-", HightLightColor.WHITE));
                    textBlock.Inlines.Add(GetTextHightLightColor(tradingHour.ClosingTime.ToString("HH:mm"), HightLightColor.RED));
                    return textBlock;
                }

                for (int i = 0; i < tradingHours.Count; i++)
                {
                    textBlock.Inlines.Add(GetTextHightLightColor(tradingHours[i].OpeningTime.ToString("HH:mm"), HightLightColor.WHITE));
                    textBlock.Inlines.Add(GetTextHightLightColor("-", HightLightColor.WHITE));
                    textBlock.Inlines.Add(GetTextHightLightColor(tradingHours[i].ClosingTime.ToString("HH:mm"), HightLightColor.WHITE));
                    if (i < tradingHours.Count - 1)
                        textBlock.Inlines.Add(GetTextHightLightColor("\n", HightLightColor.WHITE));
                }
                textBlock.Inlines.FirstOrDefault().Foreground = (Brush)_brushConverter.ConvertFromString(StaticConstants.GREEN_COLOR);
                textBlock.Inlines.Last().Foreground = (Brush)_brushConverter.ConvertFromString(StaticConstants.RED_COLOR);

                return textBlock;
            }

            return textBlock;
        }

        private Run GetTextHightLightColor(string text, HightLightColor color)
        {
            switch (color)
            {
                case HightLightColor.RED:
                    return new Run(text) { Foreground = (Brush)_brushConverter.ConvertFromString(StaticConstants.RED_COLOR) };

                case HightLightColor.GREEN:
                    return new Run(text) { Foreground = (Brush)_brushConverter.ConvertFromString(StaticConstants.GREEN_COLOR) };

                default: return new Run(text) { Foreground = Brushes.White };
            }
        }

        private TextBlock NoInfoTrading()
        {
            TextBlock textBlock = DefaultTextBlock;
            textBlock.Inlines.Add(new Run(StaticConstants.NO_TRADING_HOUR_INFO) { Foreground = (Brush)_brushConverter.ConvertFromString(StaticConstants.RED_COLOR) });
            return textBlock;
        }

        private TextBlock ClosingTrading()
        {
            TextBlock textBlock = DefaultTextBlock;
            textBlock.TextAlignment = TextAlignment.Center;
            textBlock.Inlines.Add(new Run(StaticConstants.CLOSED) { Foreground = (Brush)_brushConverter.ConvertFromString(StaticConstants.RED_COLOR) });
            return textBlock;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}