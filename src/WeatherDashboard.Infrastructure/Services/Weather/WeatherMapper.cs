namespace WeatherDashboard.Infrastructure.Services.Weather;

using Domain.Entities.Weather;
using Responses;

internal static class WeatherMapper
{
    public static Forecast Map(WeatherApiResponse source)
    {
        ArgumentNullException.ThrowIfNull(source);

        DateOnly today = DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTime.Now, source.TimeZone));
        int todayIndex = Array.IndexOf(source.Items.Times, today);
        int totalCount = source.Items.Times.Length;

        Forecast target = new()
        {
            Latitude = source.Latitude,
            Longitude = source.Longitude,
            TimeZone = source.TimeZone,
        };

        MapFromArray(source, target, todayIndex);

        target.FutureForecasts = new Dictionary<DateOnly, Forecast>(totalCount - 1);

        for ( int i = 0; i < totalCount; i++ )
        {
            if ( i == todayIndex )
            {
                continue;
            }

            Forecast forecast = new()
            {
                Latitude = source.Latitude,
                Longitude = source.Longitude,
                TimeZone = source.TimeZone,
            };

            MapFromArray(source, forecast, i);
            target.FutureForecasts[source.Items.Times[i]] = forecast;
        }

        return target;
    }

    private static void MapFromArray(WeatherApiResponse source, Forecast target, int index)
    {
        ForecastResponse items = source.Items;
        TimeSpan offset = source.TimeZone.BaseUtcOffset;

        target.DewPoint = Math.Round(items.DewPoints[index], 0, MidpointRounding.AwayFromZero);
        target.Humidity = items.Humidity[index];
        target.Sunrise = new DateTimeOffset(items.SunriseTimes[index], offset);
        target.Sunset = new DateTimeOffset(items.SunsetTimes[index], offset);
        target.SurfacePressure = items.SurfacePressures[index];
        target.Temperature = Math.Round(items.Temperatures[index], 0, MidpointRounding.AwayFromZero);
        target.UvIndex = items.UvIndexes[index];
        target.Visibility = items.Visibilities[index];
        target.WeatherCode = items.WeatherCodes[index];
        target.WindDirection = items.WindDirections[index];
        target.WindGusts = items.WindGusts[index];
        target.WindSpeed = items.WindSpeeds[index];
    }
}
