import { revalidatePath } from 'next/cache';
import { cookies } from 'next/headers';
import { redirect } from 'next/navigation';

export async function POST() {
  const cookieStore = await cookies();

  cookieStore.delete('token');
  revalidatePath('/manage-your-appointments/login');
  return redirect('/manage-your-appointments/login');
}
