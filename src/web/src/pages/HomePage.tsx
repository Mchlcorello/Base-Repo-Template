import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "../shared/api/client";

interface ApiInfo {
  service: string;
  version: string;
  environment: string;
}

export function HomePage() {
  const { data, isLoading, error } = useQuery<ApiInfo>({
    queryKey: ["api-info"],
    queryFn: () => apiFetch<ApiInfo>("/api/info"),
  });

  return (
    <main style={{ fontFamily: "system-ui, sans-serif", padding: "2rem", maxWidth: 640, margin: "0 auto" }}>
      <h1>Base Repo Template</h1>
      <p>Live data from the API at <code>/api/info</code>:</p>

      {isLoading && <p>Loading…</p>}
      {error && (
        <pre role="alert" style={{ color: "crimson" }}>
          {error instanceof Error ? error.message : "Unknown error"}
        </pre>
      )}
      {data && (
        <pre data-testid="api-info" style={{ background: "#f5f5f5", padding: "1rem", borderRadius: 6 }}>
          {JSON.stringify(data, null, 2)}
        </pre>
      )}
    </main>
  );
}
