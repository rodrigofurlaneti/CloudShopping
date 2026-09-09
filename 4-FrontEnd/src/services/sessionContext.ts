import { createContext, useContext } from 'react';
import type { SessionUser } from './storeService';
export const SessionContext = createContext<{user:SessionUser|null;loading:boolean;error:string;refresh:()=>Promise<void>;logout:()=>Promise<void>}>({user:null,loading:true,error:'',refresh:async()=>{},logout:async()=>{}});
export const useSession = () => useContext(SessionContext);
