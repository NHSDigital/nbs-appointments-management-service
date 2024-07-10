import '../../index.css'
import "/node_modules/nhsuk-frontend/dist/nhsuk.css"
import { ClientOnly } from './client'
 
export function generateStaticParams() {
  return [{ slug: [''] }]
}
 
export default function Page() {
  return <ClientOnly />
}