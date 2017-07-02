using System;
using Polly;

namespace PollyApp
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            #region Reintentar

            Console.WriteLine("== Reintentar ==");
            var politicaReintenta = Policy
                .Handle<Exception>()
                .Retry(ReportaError);

            try
            {
                politicaReintenta.Execute(LanzaExcepcion);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Después de los intentos, sigue fallando ({e.Message})");
            }

            var politicaReintenta5 = Policy
                .Handle<DivideByZeroException>()
                .Retry(5, ReportaError);

            Console.WriteLine("\n== Reintentar 5 veces ==");

            try
            {
                politicaReintenta5.Execute(LanzaExcepcion);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Después de los intentos, sigue fallando ({e.Message})");
            }

            var handleSeveralExceptions = Policy
                .Handle<ArgumentException>()
                .Or<DivideByZeroException>()
                .Retry(5, ReportaError);

            Console.WriteLine("\n== Reintentar 5 veces y maneja varias excepciones ==");

            try
            {
                handleSeveralExceptions.Execute(LanzaExcepcion);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Después de los intentos, sigue fallando ({e.Message})");
            }

            #endregion

            #region Reintentar esperando tiempo

            var politicaWaitAndRetry = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(new[]
                      {
                        TimeSpan.FromSeconds(1),
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(3)
                    }, ReportaError);

            Console.WriteLine("\n== Reintentar pero esperar segundos ==");

            try
            {
                politicaWaitAndRetry.Execute(LanzaExcepcion);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Después de los intentos, sigue fallando ({e.Message})");
            }

            var politicaWaitAndRetryExponential = Policy
                .Handle<DivideByZeroException>()
                .WaitAndRetry(5,
                              intento => TimeSpan.FromSeconds(Math.Pow(2, intento)),
                              ReportaError);

            Console.WriteLine("\n== Reintentar 5 veces esperando un tiempo exponencial ==");

            try
            {
                politicaWaitAndRetryExponential.Execute(LanzaExcepcion);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Después de los intentos, sigue fallando ({e.Message})");
            }

            #endregion

            #region Reintentar esperando tiempo
            var politicaWaitAndRetryString = Policy<string>
                .Handle<Exception>()
                .WaitAndRetry(5,
                              intento => TimeSpan.FromSeconds(Math.Pow(2, intento)),
                              ReportaError);

            Console.WriteLine("\n== Reintentar con valor de retorno ==");

            try
            {
                var resultado = politicaWaitAndRetryString.Execute(LanzaExcepcionConCadena);
                Console.WriteLine($"Resultado {resultado}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Después de los intentos, sigue fallando ({e.Message})");
            }

            var politicaWaitAndRetryStringWithFallback = Policy<string>
                .Handle<Exception>()
                .Fallback("Valor de Fallback");

            Console.WriteLine("\n== Reintentar esperando un valor de fallback ==");

            intentos = 0;
            var resultado2 = politicaWaitAndRetryStringWithFallback.Execute(LanzaExcepcionConCadena);
            Console.WriteLine($"Resultado {resultado2}");

            #endregion

            Console.WriteLine("Terminado");
            Console.Read();
        }

        static void ReportaError(DelegateResult<string> resultado, TimeSpan tiempo, int intento, Context context)
        {
            Console.WriteLine($"Intento: {intento:00} (próximo intento en: {tiempo.Seconds} segundos)\tTiempo: {DateTime.Now}");
        }

        static void ReportaError(Exception e, TimeSpan tiempo, int intento, Context contexto)
        {
            Console.WriteLine($"Intento: {intento:00} (próximo intento en: {tiempo.Seconds} segundos)\tTiempo: {DateTime.Now}");
        }

        static void ReportaError(Exception e, int intentos)
        {
            Console.WriteLine($"Intento: {intentos:00}\tTiempo: {DateTime.Now}");
        }

        static void LanzaExcepcion()
        {
            throw new DivideByZeroException();
        }


        static int intentos = 0;

        static string LanzaExcepcionConCadena()
        {
            if (intentos < 3)
            {
                intentos++;
                throw new Exception();
            }
            return "Hola";
        }
    }
}
