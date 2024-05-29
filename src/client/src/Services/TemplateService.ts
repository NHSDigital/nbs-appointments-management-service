import { TemplateAssignment, WeekTemplate } from "../Types/Schedule";
import { useAuthenticatedClient } from "./ApiClient";

export const useTemplateService = () => {

      const client = useAuthenticatedClient();

      const saveTemplate = async (template: WeekTemplate) : Promise<string> => {
        return client.post(`template`, template).then(rsp => rsp.json())
      }

      const getTemplates = async(site: string) : Promise<WeekTemplate[]> => {
        return client.get(`templates?site=${site}`).then(rsp => rsp.json().then(j => j.templates))
      }

      const saveAssignments = async(site: string, assignments: TemplateAssignment[]) : Promise<Response> => {
        var payload = {
            site,
            assignments
        }
        return client.post(`templates/assignments`, payload)
      }

      const getAssignments = async(site: string) : Promise<TemplateAssignment[]> => {
        return client.get(`templates/assignments?site=${site}`).then(rsp => rsp.json().then(j => j.assignments))
      } 

      return {saveTemplate, getTemplates, saveAssignments, getAssignments};
}
