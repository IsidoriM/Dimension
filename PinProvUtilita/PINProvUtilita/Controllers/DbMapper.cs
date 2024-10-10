using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Reflection;
using System.Data;
using System.Collections;

namespace PINProvUtilita.Controllers
{
    public class DbMapper
    {

        #region Public Methods and Operators

        public static List<T> PopulateEntities<T>(IDataReader dr)
        {
            try
            {
                List<T> entities = new List<T>();

                using (dr)
                {
                    while (dr.Read())
                    {
                        T ent = Activator.CreateInstance<T>();

                        PopulateEntity(ent, dr);
                        entities.Add(ent);
                    }
                }

                return entities;
            }
            catch (Exception ex)
            {
                throw new Exception("[DbMapper: 9] " + ex.Message, ex);
            }
            finally
            {
                if (!dr.IsClosed)
                {
                    dr.Close();
                }
            }
        }

        /// <summary>
        /// Instanzia un riferimento al tipo <c>T</c>. Il tipo di oggetto instanziato è popolato
        /// con il contenuto del <c>record</c> passato come argomento.
        /// E' supposta una corrispondenza con i campi del <c>record</c> e gli attributi del tipo
        /// di oggetto specificato con <c>T</c>.
        /// </summary>
        /// <typeparam name="T">Tipo di oggetto da mappare.</typeparam>
        /// <param name="record">Il record con i dati letti da DB.</param>
        /// <returns>Un'istanza delloggeto popolata con i valori letti da record.</returns>
        public static T PopulateEntity<T>(IDataRecord record)
        {
            return PopulateEntity(Activator.CreateInstance<T>(), record);
        }

        public static T PopulateEntity<T>(T entity, IDataRecord record)
        {
            try
            {
                if (record != null && record.FieldCount > 0)
                {
                    Type type = entity.GetType();

                    for (int i = 0; i < record.FieldCount; i++)
                    {
                        if (DBNull.Value != record[i])
                        {
                            PropertyInfo property = type.GetProperty(
                                record.GetName(i),
                                BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                            if (property != null)
                            {
                                if (property.PropertyType == typeof(DateTime?) || property.PropertyType == typeof(int?))
                                {
                                    property.SetValue(entity, record[property.Name], null);
                                }
                                else
                                {
                                    // SV: corregge alcuni problemi relativi alle
                                    // conversioni e al formato tra reader e entità.
                                    try
                                    {
                                        object value = record[property.Name];

                                        if (property.PropertyType == typeof(string))
                                        {
                                            value = value.ToString().Trim();
                                        }

                                        property.SetValue(
                                            entity,
                                            Convert.ChangeType(value, property.PropertyType),
                                            null);
                                    }
                                    catch (FormatException)
                                    {
                                    }
                                    catch (InvalidCastException)
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("[DbMapper: 7] " + ex.Message, ex);
            }

            return entity;
        }

        /// <summary>
        /// Wrapper che ritorna <c>value</c> se diverso da null o string.Empty, altrimenti ritorna <c>defaultValue</c>.
        /// </summary>
        /// <param name="value">Valore.</param>
        /// <param name="defaultValue">Valore predefinito.</param>
        /// <returns>value se esso è diverso da null altrimenti defaultValue.</returns>
        public static string ToDefaultCustom(string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value) ? defaultValue : value;
        }

        /// <summary>
        /// Wrapper che ritorna <c>value</c> se diverso zero, altrimenti ritorna <c>defaultValue</c>.
        /// </summary>
        /// <param name="value">Valore.</param>
        /// <param name="defaultValue">Valore predefinito.</param>
        /// <returns>value se esso è diverso da zero altrimenti defaultValue.</returns>
        public static int ToDefaultCustom(int value, int defaultValue)
        {
            return value == 0 ? defaultValue : value;
        }

        /// <summary>
        /// Wrapper che ritorna <c>value</c> se positivo, altrimenti ritorna <c>defaultValue</c>.
        /// </summary>
        /// <param name="value">Valore.</param>
        /// <param name="defaultValue">Valore predefinito.</param>
        /// <returns>value se esso è positivo altrimenti defaultValue.</returns>
        public static float ToDefaultCustom(float value, float defaultValue)
        {
            return value > 0 ? value : defaultValue;
        }

        /// <summary>
        /// Wrapper che ritorna <c>value</c> se positivo, altrimenti ritorna <c>defaultValue</c>.
        /// </summary>
        /// <param name="value">Valore.</param>
        /// <param name="defaultValue">Valore predefinito.</param>
        /// <returns>value se esso è positivo altrimenti defaultValue.</returns>
        public static double ToDefaultCustom(double value, double defaultValue)
        {
            return value > 0 ? value : defaultValue;
        }

        /// <summary>
        /// Wrapper che ritorna <c>value</c> se diverso da null, altrimenti <c>defaultValue</c>.
        /// </summary>
        /// <typeparam name="T">Tipo di dato di value</typeparam>
        /// <param name="value">Valore.</param>
        /// <param name="defaultValue">Valore predefinito.</param>
        /// <returns>value se esso è diverso da null, altrimenti defaultValue</returns>
        public static T ToDefaultCustom<T>(T? value, T defaultValue) where T : struct
        {
            return value ?? defaultValue;
        }

        /// <summary>
        /// To the default null.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static object ToDefaultNull<T>(T? value) where T : struct
        {
            return value ?? (object)DBNull.Value;
        }

        /// <summary>
        /// Esegue una conversione del valore in DbNull se esso vale null o string.Empty.
        /// Altrimenti restituisce semplicemente la stringa.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>DbNull se value = "" o null; value altrimenti</returns>
        public static object ToDefaultNull(string value)
        {
            return string.IsNullOrEmpty(value) ? (object)DBNull.Value : value;
        }

        /// <summary>
        /// Esegue una conversione del valore in DbNull se esso vale 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>DbNull se value=0; altrimenti value</returns>
        public static object ToDefaultNull(int value)
        {
            return value == 0 ? (object)DBNull.Value : value;
        }

        /// <summary>
        /// Esegue una conversione del valore in DbNull se esso vale 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>DbNull se value=0; altrimenti value</returns>
        public static object ToDefaultNull(float value)
        {
            return value > 0 ? value : (object)DBNull.Value;
        }

        /// <summary>
        /// Esegue una conversione del valore in DbNull se esso vale 0.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>DbNull se value=0; altrimenti value</returns>
        public static object ToDefaultNull(double value)
        {
            return value > 0 ? value : (object)DBNull.Value;
        }

        #endregion

        public class SortableComparer : IComparer
        {
            #region Fields

            private readonly bool _Ascendente;

            private readonly string _NomeCampo;

            #endregion

            // Costruttore della Classe
            #region Constructors and Destructors

            public SortableComparer(string NomeCampo, bool Ascendente)
            {
                this._NomeCampo = NomeCampo;
                this._Ascendente = Ascendente;
            }

            public SortableComparer(string NomeCampo)
            {
                // Controllo se il NomeCampo comprende anche il tipo di Ordinamento
                if (NomeCampo.IndexOf(" ") > 0)
                {
                    this._NomeCampo = NomeCampo.Substring(0, NomeCampo.IndexOf(" "));
                    this._Ascendente = false;
                }
                else
                {
                    this._NomeCampo = NomeCampo;
                    this._Ascendente = true;
                }
            }

            #endregion

            #region Public Methods and Operators

            public int Compare(object a, object b)
            {
                int returnValue = 0;

                object valueA = this.GetPropertyReference(a, this._NomeCampo);
                object valueB = this.GetPropertyReference(b, this._NomeCampo);

                Type valueTypeA = (valueA != null) ? valueA.GetType() : null;
                Type valueTypeB = (valueB != null) ? valueB.GetType() : null;

                // Se entrambe i valori sono nulli
                if (valueA == null && valueB == null)
                {
                    returnValue = 0;
                }

                    // Se il valore A è nullo
                else if (valueA == null)
                {
                    returnValue = 1;
                }

                    // Se il valore A è nullo
                else if (valueB == null)
                {
                    returnValue = -1;
                }

                    // Comparazione dei valori
                else if (valueA is IComparable && valueB is IComparable)
                {
                    returnValue =
                        (int)
                        valueTypeA.InvokeMember("CompareTo", BindingFlags.InvokeMethod, null, valueA, new[] { valueB });
                }

                // Ribalto il valore nel caso in cui abbia scelto una comparazione di tipo discendente
                if (this._Ascendente == false)
                {
                    returnValue = returnValue * -1;
                }

                return returnValue;
            }

            #endregion

            #region Methods

            protected object GetPropertyReference(object Entita, string Proprieta)
            {
                try
                {
                    Type ObjectType = null;
                    object MyObject = null;

                    // Controllo se è presente un punto oppure no
                    if (Proprieta.IndexOf(".") > 0)
                    {
                        // Classe come proprietà
                        MyObject = Entita.GetType()
                            .InvokeMember(
                                Proprieta.Substring(0, Proprieta.IndexOf(".")),
                                BindingFlags.GetProperty | BindingFlags.GetField,
                                null,
                                Entita,
                                new object[0]);
                        ObjectType = MyObject.GetType();

                        // Estrapolo la proprieta
                        Proprieta = Proprieta.Substring(Proprieta.IndexOf(".") + 1);
                    }
                    else
                    {
                        // Proprietà diretta
                        ObjectType = Entita.GetType();
                        MyObject = Entita;
                    }

                    return ObjectType.InvokeMember(
                        Proprieta,
                        BindingFlags.GetProperty | BindingFlags.GetField,
                        null,
                        MyObject,
                        new object[0]);
                }
                catch (Exception ex)
                {
                    throw new Exception("[DbMapper: 8] " + ex.Message, ex);
                }
            }

            #endregion
        }
    }
}