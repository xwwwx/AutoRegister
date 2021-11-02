# AutoRegister
**AutoRegister可以透過將Class命名成特定格式，或加上特定Attribute的方式，自動將Class及其介面加入Service容器中。**

## 掃描規則

1.含有以下屬性的類別
* ComponentAttribute
* ServiceAttribute
* RepositoryAttribute

2.類別命名結尾為以下字串
* Service
* Repository

## 生命週期

1.用以下屬屬性為實體設定生命週期
* SingletonAttribute
* ScopedAttribute
* TransientAttribute

>若未設定預設為單一

## 使用說明

1.為類別套上ComponentAttribute
```Java
[Service]
public class WeatherForecastService : IWeatherForecastService {...}
```
2.為類別套上LifeTimeAttribute
```Java
[Transient]
[Service]
public class WeatherForecastService : IWeatherForecastService {...}
```
3.在`Startup.ConfigureServices`內加入`IServiceCollection.AutoRegiste()`
```Java
public void ConfigureServices(IServiceCollection services)
{
	services.AutoRegiste();
}
```