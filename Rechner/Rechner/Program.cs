using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rechner
{
    class Program
    {
        private static ConsoleKey response;

        static void Main(string[] args)
        {
            while (true)
            {
                string eingabe;
                string eingabeKlammerMulti;
                string eingabeOhneKlammern;
                int ergebnis;
                List<int> zahlen;
                List<string> operatoren;


                Console.WriteLine("bitte gleichung eingeben");
                eingabe = Console.ReadLine();

                try
                {
                    eingabe = EingabeChecker(eingabe);
                }
                catch (System.ArgumentException e)
                {
                    Console.WriteLine(e);
                    Console.WriteLine(" ");
                    Console.WriteLine("press any key to exit");
                    Console.ReadKey();
                    break;
                }

                eingabeKlammerMulti = KlammerMulti(eingabe);
                eingabeOhneKlammern = KlammerRechner(eingabeKlammerMulti);
                zahlen = ZahlenFilter(eingabeOhneKlammern);
                operatoren = OperatorFilter(eingabeOhneKlammern);
                var punktVorStrichErgebnis = PunktVorStrichRechner(operatoren, zahlen);
                zahlen = punktVorStrichErgebnis.Item2;
                operatoren = punktVorStrichErgebnis.Item1;


                ergebnis = RechnerAusfuehren(zahlen, operatoren);

                if (ergebnis.Equals(null))
                {
                    Console.WriteLine("es ist ein fehler aufgetreten");
                }

                Console.WriteLine(ergebnis);
                do
                {
                    Console.WriteLine("noch eine Rechnung ? y/n");
                    response = Console.ReadKey(true).Key;

                } while (response != ConsoleKey.Y && response != ConsoleKey.N);

                if (response == ConsoleKey.Y)
                {
                    continue;
                }
                else if (response == ConsoleKey.N)
                {
                    break;
                }
            }
        }

        private static string EingabeChecker(string eingabe)
        {
            string regexExpression = @"[^0-9\+\-\*\/\(\)\s]";                                                   //alles was keine zahl, operator, klammer oder whitespace ist

            string[] match = Regex.Matches(eingabe, regexExpression).OfType<Match>().Select(m => string.Format(m.Value)).ToArray();
            string errorMessage = "";

            if (match.Length == 0)
            {
                return eingabe;
            }
            else
            {
                foreach(string error in match)
                {
                    errorMessage = errorMessage + error;
                }
                throw new System.ArgumentException("folgende werte sind ungültig:", errorMessage);
            }
        }

        static string KlammerMulti(string eingabe)
        {
            string regexExpression = @"(?<=\d)\(|\)(?=\d)|\)\(";                                              //Klammer direkt neben zahl(kein operator)

            string[] klammernMulti = Regex.Matches(eingabe, regexExpression).OfType<Match>().Select(m => string.Format(m.Value)).ToArray();

            foreach (string k in klammernMulti)
            {
                if (k == "(")
                {
                    Regex rgx = new Regex(@"(?<=\d+)\(+?");
                    eingabe = rgx.Replace(eingabe, "*(", 1);
                }
                if (k == ")")
                {
                    Regex rgx = new Regex(@"\)(?=\d+)");
                    eingabe = rgx.Replace(eingabe, ")*", 1);
                }
                if (k == ")(")
                {
                    Regex rgx = new Regex(@"\)\(+?");
                    eingabe = rgx.Replace(eingabe, ")*(", 1);
                }
            }

            return eingabe;
        }

        static List<string> KlammerFilter(string eingabe)
        {
            string regexExpression = @"\(([^(]*?)\)";                                              //inerste Klammer in einer Klammer

            string[] klammern = Regex.Matches(eingabe, regexExpression).OfType<Match>().Select(m => string.Format(m.Value)).ToArray();


            List<string> klammernList = new List<string>();
            foreach (string k in klammern)
            {
                klammernList.Add(k);
            }

            return klammernList;
        }

        static List<int> ZahlenFilter(string eingabe)
        {
            string regexExpression = @"\d+";                                              //D = alle Nummern, + = eine Stelle oder mehr

            var zahlen = Regex.Matches(eingabe, regexExpression).OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();

            List<int> zahlenList = new List<int>();
            foreach (int o in zahlen)
            {
                zahlenList.Add(o);
            }

            return zahlenList;
        }

        static List<string> OperatorFilter(string eingabe)
        {
            string regexExpression = "[+-/*]";                                              //alle operatoren

            string[] operatoren = Regex.Matches(eingabe, regexExpression).OfType<Match>().Select(m => string.Format(m.Value)).ToArray();


            List<string> operatorenList = new List<string>();
            foreach (string o in operatoren)
            {
                operatorenList.Add(o);
            }

            return operatorenList;
        }

        static string KlammerRechner(string eingabe)
        {
            int ergebnis;
            List<int> zahlen;
            List<string> operatoren;
            string klammerAufgabe;
            string ergebnisString;
            string eingabeOhneKlammern;
            string zwischenEingabeOhneKlammern = eingabe;
            List<string> klammerMatch = KlammerFilter(zwischenEingabeOhneKlammern);

            if (klammerMatch.Count == 0)
            {
                return eingabe;
            }

            do
            {
                klammerMatch = KlammerFilter(zwischenEingabeOhneKlammern);
                klammerAufgabe = klammerMatch[0].Substring(1, klammerMatch[0].Length - 2);
                zahlen = ZahlenFilter(klammerAufgabe);
                operatoren = OperatorFilter(klammerAufgabe);
                var punktVorStrichErgebnis = PunktVorStrichRechner(operatoren, zahlen);
                zahlen = punktVorStrichErgebnis.Item2;
                operatoren = punktVorStrichErgebnis.Item1;

                ergebnis = RechnerAusfuehren(zahlen, operatoren);
                ergebnisString = ergebnis.ToString();
                zwischenEingabeOhneKlammern = zwischenEingabeOhneKlammern.Replace(klammerMatch[0], ergebnisString);

                klammerMatch.Clear();

                klammerMatch = KlammerFilter(zwischenEingabeOhneKlammern);
            } while (klammerMatch.Count != 0);

            eingabeOhneKlammern = zwischenEingabeOhneKlammern;

            return eingabeOhneKlammern;
        }

        static Tuple<List<string>, List<int>> PunktVorStrichRechner(List<string> operatoren, List<int> zahlen)
        {
            int multiplikationszeichenIndex;
            int geteiltzeichenIndex;

            multiplikationszeichenIndex = operatoren.IndexOf("*");
            geteiltzeichenIndex = operatoren.IndexOf("/");


            while (!multiplikationszeichenIndex.Equals(-1) || !geteiltzeichenIndex.Equals(-1))
            {
                if ((multiplikationszeichenIndex < geteiltzeichenIndex && multiplikationszeichenIndex != -1) ^ geteiltzeichenIndex.Equals(-1))
                {
                    zahlen[multiplikationszeichenIndex] = RechnerSimpel(zahlen[multiplikationszeichenIndex], zahlen[multiplikationszeichenIndex + 1], operatoren[multiplikationszeichenIndex]);
                    zahlen.RemoveAt(multiplikationszeichenIndex + 1);
                    operatoren.RemoveAt(multiplikationszeichenIndex);
                }

                if ((geteiltzeichenIndex < multiplikationszeichenIndex && geteiltzeichenIndex != -1) ^ multiplikationszeichenIndex.Equals(-1))
                {
                    zahlen[geteiltzeichenIndex] = RechnerSimpel(zahlen[geteiltzeichenIndex], zahlen[geteiltzeichenIndex + 1], operatoren[geteiltzeichenIndex]);
                    zahlen.RemoveAt(geteiltzeichenIndex + 1);
                    operatoren.RemoveAt(geteiltzeichenIndex);
                }

                multiplikationszeichenIndex = operatoren.IndexOf("*");
                geteiltzeichenIndex = operatoren.IndexOf("/");
            }

            var tupleReturn = new Tuple<List<string>, List<int>>(operatoren, zahlen);
            return tupleReturn;
        }

        static int RechnerSimpel(int zahl1, int zahl2, string operatoro)
        {
            if (operatoro.Equals("+"))
            {
                return zahl1 + zahl2;
            }

            if (operatoro.Equals("-"))
            {
                return zahl1 - zahl2;
            }

            if (operatoro.Equals("*"))
            {
                return zahl1 * zahl2;
            }

            // if (operatoro.Equals("/"))
            return zahl1 / zahl2;
        }

        static int RechnerAusfuehren(List<int> zahlen, List<string> operatoren)
        {
            int n = 2;
            int operatorenZahl = operatoren.Count;
            if (operatorenZahl == 0)
            {
                return zahlen[0];
            }
            int zwischenergebnis = RechnerSimpel(zahlen[0], zahlen[1], operatoren[0]);

            if (operatorenZahl < 2)
            {
                return zwischenergebnis;
            }
            foreach (string o in operatoren.Skip(1))
            {
                zwischenergebnis = RechnerSimpel(zwischenergebnis, zahlen[n], o);
                n++;
            }

            return zwischenergebnis;
        }
    }
}

