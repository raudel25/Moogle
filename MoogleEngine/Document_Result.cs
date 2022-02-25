using System.Text;

namespace MoogleEngine;
public class Document_Result
{
    public SearchItem? Item {get; set;}
    public static int Snippet_len=20;
    public Document_Result(Document document,QueryClass query)
    { 
        //Lista para las palabras de la query presentes en el documento
        List<string> words_doc = new List<string>();
        //Lista para las palabras de la busqueda literal presentes en el documneto
        List<string> words_docLiteral = new List<string>();
        //Lista para las palabras que no estan en el documento
        List<string> words_no_doc = new List<string>();
        //Lista de posiciones para la busqueda literal
        List<int> Pos_SearchLiteral = new List<int>();
        double score= SimVectors(query, document, words_doc, words_no_doc, words_docLiteral);
        //Modificacion del Score por el operador cercania
        score += Close(words_docLiteral,document,query);
        if(!ResultSearch(words_doc, words_no_doc, words_docLiteral, Pos_SearchLiteral, score, document, query)) return;
        //Buscamos los Snippets
        (string[],int[]) aux=Snippet(words_doc, Pos_SearchLiteral, document, query);
        Item = new SearchItem(document.Title,aux.Item1,aux.Item2,score,words_no_doc);
    }
    #region Search_Result
    /// <summary>Metodo para calcular la similitud del coseno</summary>
    /// <param name="words_doc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="words_docLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="words_no_doc">Lista para las palabras de la query q no estan en el documento</param>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <returns>Score del Documento</returns>
    private static double SimVectors(QueryClass query,Document document,List<string> words_doc, List<string> words_no_doc, List<string> words_docLiteral)
    {
        double score = 0;
        double mod_query = 0, query_x_doc = 0, mod_doc = 0;
        query_x_doc = 0; mod_doc = 0;
            
            foreach (var word_dic in query.Words_Query)
            {
                //Calculamos el producto de los 2 vectores
                query_x_doc += Corpus_Data.Vocabulary[word_dic.Key].Weight_Doc[document.Index] * word_dic.Value;
                if (Corpus_Data.Vocabulary[word_dic.Key].Weight_Doc[document.Index] * word_dic.Value != 0)
                {
                    words_doc.Add(word_dic.Key);
                }
                if (Corpus_Data.Vocabulary[word_dic.Key].Pos_Doc[document.Index] != null)
                {
                    words_docLiteral.Add(word_dic.Key);
                }
                else words_no_doc.Add(word_dic.Key);
            }
            double factor = 0;
            if (words_doc.Count == 0)
            {
                //Si no tenemos resultados buscamos las raices y los sinonimos
                foreach (var word_dic in query.Words_Stemming_Syn)
                {
                    query_x_doc += Corpus_Data.Vocabulary[word_dic.Key].Weight_Doc[document.Index] * word_dic.Value[0];
                    if (Corpus_Data.Vocabulary[word_dic.Key].Weight_Doc[document.Index] * word_dic.Value[0] != 0)
                    {
                        words_doc.Add(word_dic.Key);
                    }
                }
                mod_query = query.Norma_Stemming_Syn;
                if (query_x_doc != 0) 
                {
                    factor = double.MinValue;
                }
            }
            else mod_query = query.Norma;
            mod_doc = Document.Documents![document.Index].Norma;
            if (mod_query * mod_doc != 0)
            {
                //Calculamos la similitud del coseno
                score = factor + query_x_doc / Math.Sqrt(mod_query * mod_doc);
            }
            return score;
    }
    /// <summary>Metodo para determinar si el documento puede ser resultado de la busqueda</summary>
    /// <param name="words_doc">Lista para las palabras de la query presentes en el documento</param>
    /// <param name="words_docLiteral">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="score">Score del documento</param>
    /// <param name="doc">Documento</param>
    /// <returns>Si el documento debe ser devuelto</returns>
    private static bool ResultSearch(List<string> words_doc,List<string> words_no_doc, List<string> words_docLiteral, List<int> Pos_SearchLiteral, double score, Document doc,QueryClass query)
    {
        if(score==0) return false;
        //Comprobamos el operador de Exclusion
        for (int m = 0; m < query.Exclude.Count; m++)
        {
            if (Corpus_Data.Vocabulary[query.Exclude[m]].Pos_Doc[doc.Index] != null)
            {
                return false;
            }
        }
        //Comprobamos el operador de Inclusion
        for (int m = 0; m < query.Include.Count; m++)
        {
            if (Corpus_Data.Vocabulary[query.Include[m]].Pos_Doc[doc.Index] == null)
            {
                return false;
            }
        }
        //Comprobamos el operador de busqueda literal
        return SearchLiteral_Operator(words_docLiteral, doc,query, Pos_SearchLiteral);
    }
    /// <summary>Metodo para comprobar la busqueda literal</summary>
    /// <param name="words_doc">Lista para las palabras de la busqueda literal presentes en el documneto</param>
    /// <param name="doc">Documento</param>
    /// <returns>Bool indicando si el documento es valido para el operador busqueda literal</returns>
    private static bool SearchLiteral_Operator(List<string> words_doc, Document doc,QueryClass query, List<int> Pos_SearchLiteral)
    {
        for (int i = 0; i < query.SearchLiteral_Words.Count; i++)
        {
            //Evaluamos los parametros para la busqueda literal
            for (int j = 0; j < query.SearchLiteral_Words[i].Count; j++)
            {
                if(query.SearchLiteral_Words[i][j]=="?") continue;
                if (Corpus_Data.Vocabulary[query.SearchLiteral_Words[i][j]].Pos_Doc[doc.Index] == null) return false;
            }
            int pos_literal = Distance_Word.Distance_Literal(query.SearchLiteral_Words[i], doc);
            if (pos_literal == -1) return false;
            //Guardamos la posicion de la busqueda literal encontrada
            Pos_SearchLiteral.Add(pos_literal);
        }
        return true;
    }
    /// <summary>Metodo obtener el Ranking de la cercania</summary>
    /// <param name="words">Lista de palabras</param>
    /// <param name="document">Documento</param>
    /// <returns>Cantidad a incrementar en el score por el operador cercania</returns>
    private static double Close(List<string> words, Document document,QueryClass query)
    {
        double sumScore = 0;
        foreach (var word_list in query.Close_Words)
        {
            bool close=true;
            //Comprobamos que las palabras del operador cercania esten en nuestra lista de palabras
            for (int j = 0; j < word_list.Count; j++)
            {
                if (!words.Contains(word_list[j]))
                {
                    close=false;
                    break;
                }              
            }
            //Si hemos encontrado todas las palabras de nuestro operador cercania buscamos la minima distancia
            if (close)
            {
                double n = (double)Distance_Word.Distance_Close(word_list, document);
                if(n == int.MaxValue) continue;
                sumScore += 100/(n-1);
            }
        }
        return sumScore;
    }
    #endregion
    
    #region Snippet
    /// <summary>Metodo para buscar los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="query">Query</param>
    /// <param name="Snippetwords">Lista de las posiciones de las palabras</param>
    /// <returns>Snippets y posiciones en las que de encuentran</returns>
    private static (string[],int[]) Snippet(List<string> Snippetwords, List<int> Pos_SearchLiteral, Document document, QueryClass query)
    {
        List<int> words_list = new List<int>();
        //Comprobamos si tenemos resultados de busqueda literal
        if (query.SearchLiteral_Words.Count > 0)
            {
                for (int x = 0; x < query.SearchLiteral_Words.Count; x++)
                {
                    //Removemos la palabra de la busqueda literal, de la lista de palabras del snippet
                    foreach (var y in query.SearchLiteral_Words[x])
                    {
                        if (Snippetwords.Contains(y)) Snippetwords.Remove(y);
                    }
                    //Guardamos la posicion del snippet
                    words_list.Add(Pos_SearchLiteral[x]);
                }
            }
            //Buscamos la mayor cantidad de palabras de la query que esten contenidas en una ventana de tamaño Snippet_len
            while (Snippetwords.Count != 0 && words_list.Count < 5)
            {
                (int, List<string>) tuple = Distance_Word.Distance_Snippet(Snippetwords, document);
                words_list.Add(tuple.Item1);
                //Actualizamos la lista de palabras
                Snippetwords = tuple.Item2;
            }
            return BuildSinipped(document, words_list);
    }
    /// <summary>Metodo para construir los snippets</summary>
    /// <param name="document">Documento</param>
    /// <param name="Snippetwords">Lista de las posiciones de las palabras</param>
    /// <returns>Snippets y posiciones en las que de encuentran</returns>
    private static (string[],int[]) BuildSinipped(Document document, List<int> Snippetwords)
    {
        Snippetwords.Sort();
        string[] doc = File.ReadAllLines(document.Path);
        string[] addSnippet = new string[Snippetwords.Count];
        int[] addposSnippet = new int[Snippetwords.Count];
        //Recorremos el documento
        int i=0;
        int cant = 0;
        for (int linea_ind = 0; linea_ind < doc.Length; linea_ind++)
        {
            string[] linea = doc[linea_ind].Split();
            for(int j=0;j<linea.Length;j++)
            {
                if(cant==Snippetwords[i])
                {
                    int cant_words_snippet = 0;
                    StringBuilder sb = new StringBuilder();
                    for (int w = j; w < linea.Length; w++)
                    {
                        sb.Append(linea[w] + " ");
                        cant_words_snippet++;
                        if (cant_words_snippet == Snippet_len) break;
                    }
                    //Si no hemos alcanzado la longitud de nuestro snippet vamos a la linea siguiente
                    if (cant_words_snippet < Snippet_len && linea_ind < doc.Length - 1)
                    {
                        int ind = linea_ind + 1;
                        if (doc[linea_ind + 1] == "" && linea_ind < doc.Length - 2)
                        {
                            ind = linea_ind + 2;
                        }
                        string[] newlinea = doc[ind].Split();
                        for (int w = 0; w < newlinea.Length; w++)
                        {
                            sb.Append(newlinea[w] + " ");
                            cant_words_snippet++;
                            if (cant_words_snippet == Snippet_len) break;
                        }
                    }
                    //Guardamos el snippet
                    addSnippet[i] = sb.ToString();
                    addposSnippet[i] = linea_ind;
                    //Comprobamos el proximi indice
                    i++;
                }
                if(i==Snippetwords.Count) break; 
                string word = linea[j];
                //Quitamos los signos de puntuacion
                word = Document.Sign_Puntuation(word);
                //Si solo es un signo de puntuacion seguimos
                if (word == "") continue;
                cant++;                    
            }
            if(i == Snippetwords.Count) break;                
        }
        return (addSnippet,addposSnippet);
    }
    #endregion
}