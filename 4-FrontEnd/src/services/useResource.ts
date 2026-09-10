import { useEffect, useState } from 'react';
import { request } from './http';

// Associate each result with its request so navigation never renders stale data.
export function useResource<T>(path: string | null, revision = 0) {
    const key = JSON.stringify([path, revision]);
    const [result, setResult] = useState<{key:string; data?:T; error:string}>();
    useEffect(() => {
        if (!path) return;
        const controller = new AbortController();
        // StrictMode performs setup -> cleanup -> setup synchronously in development.
        // Defer dispatch one microtask so the discarded setup never sends a request.
        // Real navigation still aborts requests that have already started.
        Promise.resolve().then(() => {
            if (controller.signal.aborted) return;
            return request<T>(path, {signal:controller.signal});
        })
            .then(data => { if (!controller.signal.aborted) setResult({key,data,error:''}); })
            .catch(error => { if (!controller.signal.aborted) setResult({key,error:error.message}); });
        return () => controller.abort();
    }, [path, key]);
    const current = result?.key === key ? result : undefined;
    return { data:current?.data, error:current?.error || '', loading:!!path && !current };
}
