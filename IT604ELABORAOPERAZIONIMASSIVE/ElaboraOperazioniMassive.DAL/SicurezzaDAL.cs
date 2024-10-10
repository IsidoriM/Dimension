using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;
using System.Data.SqlClient;
using System.Net;

namespace ElaboraOperazioniMassive.DAL
{
    public class SicurezzaDAL
    {
    
           
        public DataSet GetOperazioniMassive()
        {
            //ritorna  dataset con dati della storedProcedure SPOM_GetOperazioniMassive
            DataSet ds = new DataSet();
            string strSQL = ConfigurationManager.ConnectionStrings["SicurezzaMP"].ToString();
            SqlConnection conStringSQL = new SqlConnection(strSQL);
            try
            {

                SqlCommand cmd = new SqlCommand("spOM_GetOperazioniMassive", conStringSQL);
                cmd.CommandType = CommandType.StoredProcedure;
                //myCommandSQL.Parameters.Clear();
                SqlDataAdapter da = new SqlDataAdapter();
                
                da.SelectCommand = cmd;
                da.Fill(ds);

                return ds;
            }


            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conStringSQL.Close();
                conStringSQL.Dispose();
            }
        }

           public DataSet GetIdServiziUtenteEnte(string vcodEnte, string vUtente, string vidg, string vids)
           {
            
             

            //ritorna  dataset con dati della storedProcedure SPOM_GetOperazioniMassive
            DataSet ds = new DataSet();
            string strSQL = ConfigurationManager.ConnectionStrings["SicurezzaMP"].ToString();
            SqlConnection conStringSQL = new SqlConnection(strSQL);
            SqlParameter codEnte = new SqlParameter("@vcodEnte", SqlDbType.NVarChar);
            codEnte.Value = vcodEnte;
            SqlParameter Utente = new SqlParameter("@vUtente", SqlDbType.NVarChar);
            Utente.Value = vUtente;
            int appserv = 0;

            try
            {

                SqlCommand cmd = new SqlCommand("spOM_GetIdServiziUtenteEnte", conStringSQL);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Clear();
                cmd.Parameters.Add(codEnte);
                cmd.Parameters.Add(Utente);
                if (String.IsNullOrEmpty(vidg) == false)
                {

                    int idgs = int.Parse(vidg);
                    SqlParameter idGS = new SqlParameter("@VGservizi", SqlDbType.Int);
                    idGS.Value = idgs;
                    cmd.Parameters.Add(idGS);
                }
                else
                {
                    SqlParameter idGS = new SqlParameter("@VGservizi", SqlDbType.Int);
                    idGS.Value = 0;
                    cmd.Parameters.Add(idGS);
                }


                if (String.IsNullOrEmpty(vids) == false)
                {

                    int idvsi = int.Parse(vids);
                    SqlParameter idvs = new SqlParameter("@vservizio", SqlDbType.Int);
                    idvs.Value = idvsi;
                    cmd.Parameters.Add(idvs);
                }
                else
                {
                    
                    SqlParameter idvs = new SqlParameter("@vservizio", SqlDbType.Int);
                    idvs.Value = 0;
                    cmd.Parameters.Add(idvs);
                }



                
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                da.Fill(ds);
                return ds;
            }


            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conStringSQL.Close();
                conStringSQL.Dispose();
            }
        }

           public int UpdateData(int idoperazione)
           {
               string strSQL = ConfigurationManager.ConnectionStrings["SicurezzaMP"].ToString();
               SqlConnection conStringSQL = new SqlConnection(strSQL);
               conStringSQL.Open();
               SqlParameter idop = new SqlParameter("@idoperazione", SqlDbType.Int);
               idop.Value = idoperazione;

               try
               {

                   SqlCommand cmd = new SqlCommand("spOM_UpdateData", conStringSQL);
                   cmd.CommandType = CommandType.StoredProcedure;
                   cmd.Parameters.Clear();
                   cmd.Parameters.Add(idop);
                 //SqlDataAdapter da = new SqlDataAdapter();
                 //da.SelectCommand = cmd;
                   cmd.ExecuteNonQuery();

               }


               catch (Exception ex)
               {
                   throw ex;
                   return -1;
               }
               finally
               {
                   conStringSQL.Close();
                   conStringSQL.Dispose();
                   
               }


               return 1;

           
           }


    }


}

    

   