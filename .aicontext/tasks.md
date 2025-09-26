# 1) Цель и функционал

**Цель:** настольное WPF-приложение, показывающее:

* Текущие условия (температура, ощущаемая, влажность, ветер, облачность, код погоды/иконка).
* Прогноз на ближайшие **3 дня** (мин/макс температуры, суммарные осадки, код погоды, максимум порывов ветра).

**Город/координаты:** Chuhuiv, Kharkiv Oblast, Ukraine — `lat=49.836626`, `lon=36.689939`.

**Источник данных:** Open-Meteo Forecast API, ответ в **FlatBuffers** + дешифровка `openmeteo_sdk` для C#. (SDK даёт скомпилированные FlatBuffers-схемы; HTTP-запрос делаем сами и добавляем `&format=flatbuffers`.) ([nuget.org][1])

# 2) Техстек и зависимости

* **.NET 9**, **WPF** (Windows), **C# 13** (по умолчанию с SDK).
* MVVM (желательно **CommunityToolkit.Mvvm**).
* HTTP: `HttpClient` (собираем URL на Open-Meteo).
* **NuGet**:

  * `openmeteo_sdk` (FlatBuffers схемы от Open-Meteо). ([nuget.org][1])
  * `Google.FlatBuffers` (подтянется транзитивно из `openmeteo_sdk`). ([nuget.org][1])
  * `CommunityToolkit.Mvvm` (удобные атрибуты, `ObservableObject`, `RelayCommand`).
  * (опц.) `Serilog` + `Serilog.Sinks.File` для логов.

> Прелесть Open-Meteo — **без API-ключа** на некоммерческом использовании, глобальное покрытие, JSON/FlatBuffers, гибкая выборка полей. ([open-meteo.com][2])

# 3) Архитектура проекта

```
/src/ChuhuivWeather.App
  /App.xaml, App.xaml.cs
  /Views/MainWindow.xaml, MainWindow.xaml.cs
  /ViewModels/MainViewModel.cs
  /Services/OpenMeteoWeatherService.cs
  /Models/WeatherModels.cs
  /Converters/WeatherCodeToIconConverter.cs
  /Assets/Icons/*.png (или Segoe MDL2 Assets / FluentEmoji)
  /Infrastructure/Http/HttpClientFactory.cs (опц.)
```

**MVVM-потоки данных:**

* `MainViewModel` → дергает `OpenMeteoWeatherService`.
* `OpenMeteoWeatherService`:

  * Формирует URL `/v1/forecast?latitude=...&longitude=...&current=...&daily=...&timezone=Europe/Kyiv&forecast_days=3&format=flatbuffers`
  * Делает GET, читает байты, декодирует FlatBuffers через `openmeteo_sdk.WeatherApiResponse`.
  * Маппит FlatBuffers в доменные модели (`CurrentConditions`, `DailyForecast[]`).
* View связывает XAML-биндинги с `MainViewModel`.

# 4) Запрос к Open-Meteo (параметры)

**Endpoint:** `https://api.open-meteo.com/v1/forecast`

**Обязательные параметры для нашей задачи:**

* `latitude=49.836626&longitude=36.689939`
* `timezone=Europe/Kyiv` (чтобы получить локальное время). ([open-meteo.com][3])
* `forecast_days=3`
* `current=`: `temperature_2m,apparent_temperature,relative_humidity_2m,weather_code,cloud_cover,pressure_msl,wind_speed_10m,wind_direction_10m,wind_gusts_10m`
* `daily=`: `weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_gusts_10m_max`
* `format=flatbuffers` (критично для использования `openmeteo_sdk`). ([GitHub][4])

> Примечание: параметр `&current=`/`&daily=` — это именно списки переменных, поддерживаемые API; см. доки и таблицы переменных. ([open-meteo.com][3])

**Пример итогового URL:**

```
https://api.open-meteo.com/v1/forecast?
  latitude=49.836626&longitude=36.689939&
  timezone=Europe/Kyiv&
  forecast_days=3&
  current=temperature_2m,apparent_temperature,relative_humidity_2m,weather_code,cloud_cover,pressure_msl,wind_speed_10m,wind_direction_10m,wind_gusts_10m&
  daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_gusts_10m_max&
  format=flatbuffers
```

# 5) Модели (Domain)

```csharp
public sealed record CurrentConditions(
    DateTimeOffset TimeLocal,
    double TemperatureC,
    double ApparentTemperatureC,
    int RelativeHumidityPct,
    int WeatherCode,
    int CloudCoverPct,
    double PressureMslHpa,
    double WindSpeedKmh,
    int WindDirectionDeg,
    double WindGustKmh);

public sealed record DailyForecast(
    DateOnly DateLocal,
    int WeatherCode,
    double TminC,
    double TmaxC,
    double PrecipitationSumMm,
    double WindGustMaxKmh);

public sealed record WeatherSnapshot(
    string LocationName, // "Chuhuiv, Kharkiv Oblast"
    CurrentConditions Current,
    IReadOnlyList<DailyForecast> Next3Days);
```

# 6) Сервис интеграции (`OpenMeteoWeatherService`)

Основные шаги:

1. Собрать URL (см. выше).
2. `HttpClient.GetByteArrayAsync(url)`.
3. Передать байты в FlatBuffers-декодер из `openmeteo_sdk` — получить `WeatherApiResponse`.
4. Разобрать `current` и `daily` (`VariablesWithTime` + массивы значений). Учитывать `utc_offset_seconds` для локального времени. ([GitHub][4])
5. Смаппить в наши модели.

> В `openmeteo_sdk` главное — типы из схемы: `WeatherApiResponse`, `VariablesWithTime`, `VariableWithValues` и т.д., как описано в README SDK. Там же упомянуто, что для FlatBuffers нужно добавить `&format=flatbuffers`. ([GitHub][4])

# 7) UI/UX (WPF)

* **MainWindow.xaml**: две колонки

  * Левая карточка (текущая погода): крупная температура, иконка по WMO-коду, «ощущается как», влажность, ветер, порывы, давление, облачность, время обновления.
  * Правая область: три тайла по дням: дата, мин/макс, иконка, осадки, макс. порыв.
* Тема: светлая/тёмная (по желанию), адаптивные размеры шрифта.
* Иконки: маппинг WMO weather code → ресурс (png/svg) или Emoji.
* Кнопка «Обновить», автообновление раз в N минут (по умолчанию 30–60 мин; Open-Meteo советует ≈час). ([open-meteo.com][5])
* Обработка оффлайна: показать «Нет сети / повторить».

# 8) Обработка WMO Weather Code

Мини-таблица маппинга (можно расширять):

* `0` — ясно ☀️
* `1,2,3` — переменная облачность/пасмурно 🌤/☁️
* `45,48` — туман 🌫
* `51,53,55` — морось 🌦
* `61,63,65` — дождь 🌧
* `71,73,75` — снег 🌨
* `80,81,82` — ливни 🌧
* `95` — гроза ⛈
  (Полная таблица в доках Open-Meteо.) ([open-meteo.com][3])

# 9) Локализация/время/единицы

* Язык UI: RU/UK (ресурсы в `Resources/*.resx`).
* Время: локализуем с учётом `utc_offset_seconds` из ответа; но при `timezone=Europe/Kyiv` Open-Meteo уже отдаёт локальные временные ряды — проверяем и приводим к `DateTimeOffset`. ([open-meteo.com][3])
* Единицы: температура — °C, ветер — km/h, осадки — mm, давление — hPa.

# 10) Ошибки и ретраи

* Таймаут HTTP, 5–10 с; 1–2 ретрая с экспоненциальной паузой (Policy-подход, Polly — опционально).
* Если пришёл JSON-объект с ошибкой (вдруг забыли `format=flatbuffers` или опечатка в параметрах) — показываем сообщение пользователю. ([open-meteo.com][3])
* Логи в файл (`logs/app.log`).

# 11) Кэш

* Последний удачный снимок в `%LOCALAPPDATA%\ChuhuivWeather\cache.bin` (сам сериализуй модели через `System.Text.Json` или protobuf). При старте — показываем кэш, параллельно обновляем онлайн.

# 12) Примерные NuGet-команды

```powershell
dotnet new wpf -n ChuhuivWeather.App -f net9.0
cd ChuhuivWeather.App
dotnet add package openmeteo_sdk
dotnet add package CommunityToolkit.Mvvm
# (опц.) dotnet add package Serilog Serilog.Sinks.File
```

(Пакет `openmeteo_sdk` совместим с .NET 6/8/9 и содержит зависимости на `Google.FlatBuffers`.) ([nuget.org][1])

# 13) Скелет сервиса (псевдокод C#)

```csharp
public sealed class OpenMeteoWeatherService
{
    private readonly HttpClient _http;

    public OpenMeteoWeatherService(HttpClient http) => _http = http;

    public async Task<WeatherSnapshot> GetAsync(CancellationToken ct)
    {
        var url =
          "https://api.open-meteo.com/v1/forecast" +
          "?latitude=49.836626&longitude=36.689939" +
          "&timezone=Europe/Kyiv&forecast_days=3&format=flatbuffers" +
          "&current=temperature_2m,apparent_temperature,relative_humidity_2m,weather_code,cloud_cover,pressure_msl,wind_speed_10m,wind_direction_10m,wind_gusts_10m" +
          "&daily=weather_code,temperature_2m_max,temperature_2m_min,precipitation_sum,wind_gusts_10m_max";

        var bytes = await _http.GetByteArrayAsync(url, ct);

        // Декодирование FlatBuffers через openmeteo_sdk:
        // using openmeteo_sdk; using Google.FlatBuffers;
        // var bb = new ByteBuffer(bytes);
        // var resp = WeatherApiResponse.GetRootAsWeatherApiResponse(bb);
        // Далее: извлечь utc_offset_seconds, current.variables[], daily.variables[] и смаппить.

        // Пример: из current найти VariableWithValues где variable == temperature (2m),
        // value -> double; для daily: arrays values[] по датам с шагом 86400 c.
        // Время daily вычисляется как start..end c interval=86400 (Unix GMT), но timezone=Europe/Kyiv даёт локальные ряды.
        // ...
        throw new NotImplementedException(); // здесь маппинг в WeatherSnapshot
    }
}
```

Сигнатуры и структура `WeatherApiResponse/VariablesWithTime/VariableWithValues` — см. README SDK (разделы `WeatherApiResponse`, `VariablesWithTime`, `VariableWithValues`). ([GitHub][4])

# 14) MainViewModel (набросок)

```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly OpenMeteoWeatherService _svc;

    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _location = "Chuhuiv, Kharkiv Oblast";
    [ObservableProperty] private WeatherSnapshot? _snapshot;
    [ObservableProperty] private string? _error;

    public IAsyncRelayCommand RefreshCommand { get; }

    public MainViewModel(OpenMeteoWeatherService svc)
    {
        _svc = svc;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
    }

    private async Task RefreshAsync()
    {
        IsBusy = true;
        Error = null;
        try { Snapshot = await _svc.GetAsync(CancellationToken.None); }
        catch (Exception ex) { Error = ex.Message; }
        finally { IsBusy = false; }
    }
}
```

# 15) MainWindow.xaml (идея)

```xml
<Window ... Title="Chuhuiv Weather" Width="960" Height="600">
  <Grid Margin="24">
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="2*"/>
      <ColumnDefinition Width="3*"/>
    </Grid.ColumnDefinitions>

    <!-- Текущая погода -->
    <Border Grid.Column="0" Padding="20" CornerRadius="16">
      <StackPanel>
        <TextBlock Text="{Binding Location}" FontSize="20" FontWeight="SemiBold"/>
        <StackPanel Orientation="Horizontal" Margin="0,8,0,0">
          <TextBlock Text="{Binding Snapshot.Current.TemperatureC, StringFormat={}{0:F0}°C}" FontSize="56" FontWeight="Bold"/>
          <TextBlock Text="{Binding Snapshot.Current.WeatherCode, Converter={StaticResource WeatherCodeToEmojiConverter}}" FontSize="56" Margin="12,0,0,0"/>
        </StackPanel>
        <TextBlock Text="{Binding Snapshot.Current.ApparentTemperatureC, StringFormat=Ощущается как {0:F0}°C}" Margin="0,4,0,0"/>
        <TextBlock Text="{Binding Snapshot.Current.RelativeHumidityPct, StringFormat=Влажность {0}%}"/>
        <TextBlock Text="{Binding Snapshot.Current.WindSpeedKmh, StringFormat=Ветер {0:F0} км/ч}"/>
        <TextBlock Text="{Binding Snapshot.Current.PressureMslHpa, StringFormat=Давление {0:F0} гПа}"/>
        <Button Content="Обновить" Command="{Binding RefreshCommand}" Margin="0,16,0,0" />
        <TextBlock Text="{Binding Error}" Foreground="Tomato" />
      </StackPanel>
    </Border>

    <!-- 3 дня -->
    <ItemsControl Grid.Column="1" ItemsSource="{Binding Snapshot.Next3Days}" Margin="16,0,0,0">
      <ItemsControl.ItemTemplate>
        <DataTemplate>
          <Border Padding="16" Margin="0,0,0,12" CornerRadius="12">
            <StackPanel Orientation="Horizontal" VerticalAlignment="Center">
              <TextBlock Text="{Binding DateLocal, StringFormat={}{0:ddd, dd MMM}}" FontSize="18" Width="140"/>
              <TextBlock Text="{Binding WeatherCode, Converter={StaticResource WeatherCodeToEmojiConverter}}" FontSize="28" Width="40"/>
              <TextBlock Text="{Binding TminC, StringFormat=мин {0:F0}°}" Width="80"/>
              <TextBlock Text="{Binding TmaxC, StringFormat=макс {0:F0}°}" Width="90"/>
              <TextBlock Text="{Binding PrecipitationSumMm, StringFormat=осадки {0:F1} мм}" Width="140"/>
              <TextBlock Text="{Binding WindGustMaxKmh, StringFormat=порывы {0:F0} км/ч}" Width="140"/>
            </StackPanel>
          </Border>
        </DataTemplate>
      </ItemsControl.ItemTemplate>
    </ItemsControl>
  </Grid>
</Window>
```

# 16) Тесты и валидация

* Юнит-тест на разбор FlatBuffers (мок-файл ответа).
* Юнит-тест преобразования WMO code → иконка/emoji.
* Интеграционный тест: реальный запрос к Open-Meteo с фиксированными координатами Чугуева, проверка не пустых значений.

# 17) Производительность и стабильность

* FlatBuffers экономит CPU/память на больших массивах (SDK именно про это). Это особенно полезно, если потом захочешь расширить до исторических данных/почасовой графики. ([GitHub][4])
* Обновление штатно раз в 30–60 мин (как рекомендуют, чаще нет смысла). ([open-meteo.com][5])

# 18) Лицензии/правила

* Open-Meteo: бесплатное некоммерческое использование, без ключа. Для коммерции — план и другой хост (`customer-…`) + `apikey`. Проверь «Pricing». ([open-meteo.com][5])

---

## Мини-чеклист «собрать и поехать»

1. `dotnet new wpf -f net9.0`
2. `dotnet add package openmeteo_sdk` (подтянет FlatBuffers) ([nuget.org][1])
3. `dotnet add package CommunityToolkit.Mvvm`
4. Реализовать `OpenMeteoWeatherService` (HTTP + FlatBuffers → модели).
5. `MainViewModel` с `RefreshCommand`.
6. Разметка `MainWindow.xaml` как выше (иконки по WMO).
7. Таймер автообновления и кэш.
8. Локализация RU/UK, `timezone=Europe/Kyiv`.

[1]: https://www.nuget.org/packages/openmeteo_sdk/1.18.6 "
        NuGet Gallery
        \| openmeteo_sdk 1.18.6
    "
[2]: https://open-meteo.com/?utm_source=chatgpt.com "Open-Meteo.com: 🌤️ Free Open-Source Weather API"
[3]: https://open-meteo.com/en/docs "️ Docs | Open-Meteo.com"
[4]: https://github.com/open-meteo/sdk "GitHub - open-meteo/sdk: Open-Meteo schema files"
[5]: https://open-meteo.com/en/pricing?utm_source=chatgpt.com "Pricing"
