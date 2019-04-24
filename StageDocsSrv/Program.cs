using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using System;
using System.Collections.Generic;
using System.IO;
using log4net;

namespace StageDocsSrv
{
    class Program
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            string newFileName = null;

            try
            {
                var apiClient = new ApiClient();
                SuppDocsFinder suppDoc = new SuppDocsFinder(apiClient);

                DirectoryInfo pickupDir = new DirectoryInfo(DSConfig.PickupDir);

                foreach (var envDir in pickupDir.GetDirectories("*", SearchOption.AllDirectories))
                {
                    //Get Supplemental Document Name
                    //Copy each file in the directory to the ingestion directory (except Supp Doc)
                    if (!envDir.Name.Equals(".downloaded"))
                    {
                        if (DSConfig.excSuppDocs.Equals("TRUE"))
                        {
                            //get the supplemental document name
                            String docName = suppDoc.GetDocName(envDir.Name);

                            if (docName.Equals("NoSuppDocFound"))
                            {
                                Log.Info("Supplement Document Not Found for the following Envelope :: " + envDir.Name);
                                //for each file in the envelope
                                foreach (var file in envDir.GetFiles())
                                {
                                    newFileName = envDir.Name + "_" + file.Name;
                                    file.MoveTo(DSConfig.DropoffDir + "\\" + newFileName);
                                }
                            }
                            else
                            {
                                Log.Info("Supplement Document Found for the following Envelope :: " + envDir.Name);
                                Log.Info("Supplement Document Name :: " + docName);
                                foreach (var file in envDir.GetFiles())
                                {
                                    //check if suppDocName match the file name
                                    if (file.Name.Equals(docName + ".pdf"))
                                    {
                                        file.Delete();
                                        Log.Info("The Following File was Identified as a Supplemental Document and was successfully deleted - Document Name :: " + docName + ".pdf");
                                    }
                                    else
                                    {
                                        newFileName = envDir.Name + "_" + file.Name;
                                        file.MoveTo(DSConfig.DropoffDir + "\\" + newFileName);
                                    }
                                }
                            }
                            envDir.Delete();

                        }
                        else
                        {
                            foreach (var file in envDir.GetFiles())
                            {
                                newFileName = envDir.Name + "_" + file.Name;
                                file.MoveTo(DSConfig.DropoffDir + "\\" + newFileName);
                            }
                        }
                    }
                }
            }
            catch (ApiException e)
            {
                Log.Error("\nDocuSign Exception!");

                // Special handling for consent_required
                String message = e.Message;
                if (!String.IsNullOrWhiteSpace(message) && message.Contains("consent_required"))
                {
                    String consent_url = String.Format("\n    {0}/oauth/auth?response_type=code&scope={1}&client_id={2}&redirect_uri={3}",
                        DSConfig.AuthenticationURL, DSConfig.PermissionScopes, DSConfig.ClientID, DSConfig.OAuthRedirectURI);

                    Log.Error("C O N S E N T   R E Q U I R E D");
                    Log.Error("Ask the user who will be impersonated to run the following url: ");
                    Log.Error(consent_url);
                }
                else
                {

                    Log.Error("Error Reponse: {0}", e.ErrorContent);
                }
            }

            catch (DirectoryNotFoundException dirEx)
            {
                Log.Error("Directory not found: " + dirEx.Message);
            }
            catch (IOException IOEx)
            {
                Log.Error("File not found or could not be Moved / Deleted: " + IOEx.Message);
            }
        }
    }
}
