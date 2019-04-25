using DocuSign.eSign.Api;
using DocuSign.eSign.Client;
using DocuSign.eSign.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StageDocsSrv
{
    internal class SuppDocsFinder : Base
    {

        public SuppDocsFinder(ApiClient apiClient) : base(apiClient)
        {
        }

        internal String GetDocName(String envelopeId)
        {
            String docName = null;
            int count = 0;
            CheckToken();

            EnvelopesApi envelopesApi = new EnvelopesApi(ApiClient.Configuration);
            EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(AccountID, envelopeId);

            foreach (EnvelopeDocument doc in docsList.EnvelopeDocuments)
            {
                if (!String.IsNullOrEmpty(doc.Display))
                {
                    if (doc.Display.Equals("modal"))
                    {
                        docName = doc.Name;
                        count++;
                    }
                }
            }

            if (count > 0)
                return docName;
            else return "NoSuppDocFound";

        }

        internal List<String> GetDocNames(String envelopeId)
        {
            List<String> docNames = new List<String>();
          //  int count = 0;
            CheckToken();

            EnvelopesApi envelopesApi = new EnvelopesApi(ApiClient.Configuration);
            EnvelopeDocumentsResult docsList = envelopesApi.ListDocuments(AccountID, envelopeId);

            foreach (EnvelopeDocument doc in docsList.EnvelopeDocuments)
            {
                if (!String.IsNullOrEmpty(doc.Display))
                {
                    if (doc.Display.Equals("modal"))
                    {
                        if (doc.Name.EndsWith("docx"))
                            docNames.Add(doc.Name + ".pdf");
                        else
                            docNames.Add(doc.Name);
                     //   count++;
                    }
                }
            }

            if (docNames.Count > 0)
                return docNames;
            else
            {
                docNames.Add("NoSuppDocFound");
                return docNames;
            }

        }
    }
}
