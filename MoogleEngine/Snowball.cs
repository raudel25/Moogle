namespace MoogleEngine;

//Algoritmo Snowball
//https://snowballstem.org/algorithms/spanish/stemmer.html
public static class Snowball
{
    /// <summary>Metodo para extraer la raiz de una palabra</summary>
    /// <param name="word">Palabra para extraer la raiz</param>
    /// <returns>Raiz de la palabra</returns>
    public static string Stemmer(string word)
    {
        var t = R1R2Rv(word);
        var r1 = t.Item1;
        var r2 = t.Item2;
        var rv = t.Item3;
        //Vamos evaluando cada uno de los pasos si la palabra no tiene cambio continuamos con otro
        var word1 = Step0(word, rv);
        if (word1 == word) word1 = Step1(word, r1, r2);
        if (word1 == word) word1 = Step2A(word, rv);
        if (word1 == word) word1 = Step2B1(word, r1, rv);
        if (word1 == word) word1 = Step2B2(word, r1, rv);
        //Realizamos siempre el paso 3
        word1 = Step3A(word1, r1, rv);
        if (word1 == word) word1 = Step3B(word, rv);
        return word1;
    }

    /// <summary>Metodo para determinar las regiones del lemmatizador</summary>
    /// <returns>Numero de caracteres desde el inicio de la palabra no contenidos en la region indicada</returns>
    private static (int, int, int) R1R2Rv(string word)
    {
        var r1 = word.Length;
        var r2 = word.Length;
        var rv = word.Length;
        int i;
        //r1 es la region despues de la primera consonante antecedida por una vocal
        for (i = 1; i < word.Length; i++)
            if (Data.Vocals.Contains(word[i - 1]) && !Data.Vocals.Contains(word[i]))
            {
                r1 = i + 1;
                break;
            }

        //r2 es la region despues de la primera consonante antecedida por una vocal en r1
        for (var j = i + 2; j < word.Length; j++)
            if (Data.Vocals.Contains(word[j - 1]) && !Data.Vocals.Contains(word[j]))
            {
                r2 = j + 1;
                break;
            }

        if (word.Length > 2)
        {
            //Si la 2da letra es una consonante rv es la region despues de la siguiente vocal
            if (!Data.Vocals.Contains(word[1]))
            {
                for (var j = 2; j < word.Length; j++)
                    if (Data.Vocals.Contains(word[j]))
                    {
                        rv = j + 1;
                        break;
                    }
            }
            else if (Data.Vocals.Contains(word[1]) && Data.Vocals.Contains(word[0]))
            {
                //Si las dos primeras letras son vocales rv es la region despues de la siguiente vocal
                for (var j = 2; j < word.Length; j++)
                    if (!Data.Vocals.Contains(word[j]))
                    {
                        rv = j + 1;
                        break;
                    }
            }
            else
            {
                //En caso contrario rv es la region a partir de la 3ra letra
                rv = 3;
            }
        }

        return new ValueTuple<int, int, int>(r1, r2, rv);
    }

    /// <summary>Pronombre adjunto, gerundios e infinitivos</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step0(string word, int rv)
    {
        var index = word.Length;
        var find = false;
        int i;
        for (i = 3; i >= 1; i--)
            if (word.Length - 1 - i >= 0)
                if (Data.Step0.Contains(word.Substring(word.Length - 1 - i)))
                {
                    find = true;
                    break;
                }

        if (find)
        {
            for (var j = 4; j >= 1; j--)
                if (word.Length - 2 - i - j >= 0)
                    if (Data.AfterStep0.Contains(word.Substring(word.Length - 2 - i - j, j + 1)))
                    {
                        index = word.Length - 2 - i - j;
                        break;
                    }
        }
        else
        {
            for (var j = 4; j >= 1; j--)
                if (word.Length - 2 - i - j >= 0)
                    if (Data.AfterStep0.Contains(word.Substring(word.Length - 1 - j, j + 1)))
                    {
                        index = word.Length - 1 - j;
                        break;
                    }
        }

        if (index < rv) return word;
        return word.Substring(0, index);
    }

    /// <summary>Eliminación de sufijos estándar</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step1(string word, int r1, int r2)
    {
        var index = word.Length;
        for (var i = 6; i >= 1; i--)
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
                if (Data.Step1.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }

        if (index < r2) return word;
        return word.Substring(0, index);
    }

    /// <summary>Sufijos verbales que empiezan por y</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step2A(string word, int rv)
    {
        var index = word.Length;
        for (var i = 4; i >= 1; i--)
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= rv)
                if (Data.Step2A.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }

        if (index < rv) return word;
        return word.Substring(0, index);
    }

    /// <summary>Otros sufijos verbales</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step2B1(string word, int r1, int rv)
    {
        var index = word.Length;
        for (var i = 3; i >= 1; i--)
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
                if (Data.Step2B1.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }

        if (index - 2 >= 0 && index > rv)
        {
            if (word[index - 1] == 'u' && (word[index - 2] == 'g' || word[index - 2] == 'q')) index--;
        }
        else
        {
            if (index < rv) return word;
        }

        return word.Substring(0, index);
    }

    /// <summary>Otros sufijos verbales</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step2B2(string word, int r1, int rv)
    {
        var index = word.Length;
        for (var i = 6; i >= 1; i--)
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
                if (Data.Step2B2.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }

        if (index < rv) return word;
        return word.Substring(0, index);
    }

    /// <summary>Sufijo residual</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step3A(string word, int r1, int rv)
    {
        var index = word.Length;
        for (var i = 1; i >= 0; i--)
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
                if (Data.Step3A.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }

        if (index < rv) return word;
        return word.Substring(0, index);
    }

    /// <summary>Sufijo residual</summary>
    /// <returns>Raiz de la palabra luego de realizar el paso</returns>
    private static string Step3B(string word, int rv)
    {
        var index = word.Length;
        if (Data.Step3B.Contains(word.Substring(word.Length - 1))) index = word.Length - 1;

        if (index - 2 >= 0 && index > rv)
        {
            if (word[index - 1] == 'u' && (word[index - 2] == 'g' || word[index - 2] == 'q')) index--;
        }
        else
        {
            if (index < rv) return word;
        }

        return word.Substring(0, index);
    }
}