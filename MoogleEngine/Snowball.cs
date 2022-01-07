namespace MoogleEngine;
public static class Snowball
{
    public static string Stemmer(string word)
    {
        Tuple<int, int, int> t = R1_R2_RV(word);
        int r1 = t.Item1;
        int r2 = t.Item2;
        int rv = t.Item3;
        string word1 = Step0(word, r1, r2, rv);
        if (word1 == word) word1 = Step1(word, r1, r2, rv);
        if (word1 == word) word1 = Step2_a(word, r1, r2, rv);
        if (word1 == word) word1 = Step2_b1(word, r1, r2, rv);
        if (word1 == word) word1 = Step2_b2(word, r1, r2, rv);
        word1 = Step3_a(word1, r1, r2, rv);
        if (word1 == word) word1 = Step3_b(word, r1, r2, rv);
        return word1;
    }
    static Tuple<int, int, int> R1_R2_RV(string word)
    {
        int r1 = word.Length;
        int r2 = word.Length;
        int rv = word.Length;
        int i = 0;
        for (i = 1; i < word.Length; i++)
        {
            if (Data.Vocals.Contains(word[i - 1]) && !Data.Vocals.Contains(word[i]))
            {
                //if (i < word.Length - 1) r1 = word.Substring(i + 1);
                r1 = i + 1;
                break;
            }
        }
        for (int j = i + 2; j < word.Length; j++)
        {
            if (Data.Vocals.Contains(word[j - 1]) && !Data.Vocals.Contains(word[j]))
            {
                //if (j < word.Length - 1) r2 = word.Substring(j + 1);
                r2 = j + 1;
                break;
            }
        }
        if (word.Length > 2)
        {
            if (!Data.Vocals.Contains(word[1]))
            {
                for (int j = 2; j < word.Length; j++)
                {
                    if (Data.Vocals.Contains(word[j]))
                    {
                        //if (j < word.Length - 1) rv = word.Substring(j + 1);
                        rv = j + 1;
                        break;
                    }
                }
            }
            else if (Data.Vocals.Contains(word[1]) && Data.Vocals.Contains(word[0]))
            {
                for (int j = 2; j < word.Length; j++)
                {
                    if (!Data.Vocals.Contains(word[j]))
                    {
                        //if (j < word.Length - 1) rv = word.Substring(j + 1);
                        rv = j + 1;
                        break;
                    }
                }
            }
            else
            {
                //rv = word.Substring(3);
                rv = 3;
            }
        }
        return new Tuple<int, int, int>(r1, r2, rv);
    }
    static string Step0(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        bool encontrado = false;
        int i = 0;
        for (i = 3; i >= 1; i--)
        {
            if (word.Length - 1 - i >= 0)
            {
                if (Data.Step0.Contains(word.Substring(word.Length - 1 - i)))
                {
                    encontrado = true;
                    break;
                }
            }
        }
        if (encontrado)
        {
            for (int j = 4; j >= 1; j--)
            {
                if (word.Length - 2 - i - j >= 0)
                {
                    if (Data.AfterStep0.Contains(word.Substring(word.Length - 2 - i - j, j + 1)))
                    {
                        index = word.Length - 2 - i - j;
                        break;
                    }
                }
            }
        }
        else
        {
            for (int j = 4; j >= 1; j--)
            {
                if (word.Length - 2 - i - j >= 0)
                {
                    if (Data.AfterStep0.Contains(word.Substring(word.Length - 1 - j, j + 1)))
                    {
                        index = word.Length - 1 - j;
                        break;
                    }
                }
            }
        }
        return word.Substring(0, index);
        //return index + "";
    }
    static string Step1(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        for (int i = 6; i >= 1; i--)
        {
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
            {
                if (Data.Step1.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }
            }
        }
        return word.Substring(0, index);
        //return index + "";
    }
    static string Step2_a(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        for (int i = 4; i >= 1; i--)
        {
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= rv)
            {
                if (Data.Step2_a.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }
            }
        }
        return word.Substring(0, index);
    }
    static string Step2_b1(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        for (int i = 3; i >= 1; i--)
        {
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
            {
                if (Data.Step2_b1.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }
            }
        }
        if (index - 2 >= 0 && index > r1)
        {
            if ((word[index - 1] == 'u') && (word[index - 2] == 'g' || word[index - 2] == 'q'))
            {
                index--;
            }
        }
        return word.Substring(0, index);
    }
    static string Step2_b2(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        for (int i = 6; i >= 1; i--)
        {
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
            {
                if (Data.Step2_b2.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }
            }
        }
        return word.Substring(0, index);
    }
    static string Step3_a(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        for (int i = 1; i >= 0; i--)
        {
            if (word.Length - 1 - i >= 0 && word.Length - 1 - i >= r1)
            {
                if (Data.Step3_a.Contains(word.Substring(word.Length - 1 - i)))
                {
                    index = word.Length - 1 - i;
                    break;
                }
            }
        }
        return word.Substring(0, index);
        //return index + "";
    }
    static string Step3_b(string word, int r1, int r2, int rv)
    {
        int index = word.Length;
        if (Data.Step3_b.Contains(word.Substring(word.Length - 1)))
        {
            index = word.Length - 1;
        }
        if (index - 2 >= 0 && index > r1)
        {
            if ((word[index - 1] == 'u') && (word[index - 2] == 'g' || word[index - 2] == 'q'))
            {
                index--;
            }
        }
        return word.Substring(0, index);
        //return index + "";
    }
}
