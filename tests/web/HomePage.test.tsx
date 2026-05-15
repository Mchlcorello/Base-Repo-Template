import { render, screen, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { HomePage } from "../../src/web/src/pages/HomePage";

function renderHomePage() {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false } },
  });
  return render(
    <QueryClientProvider client={client}>
      <HomePage />
    </QueryClientProvider>,
  );
}

describe("HomePage", () => {
  beforeEach(() => {
    vi.stubGlobal(
      "fetch",
      vi.fn(async () =>
        new Response(
          JSON.stringify({ service: "Base.Api", version: "9.9.9", environment: "Test" }),
          { status: 200, headers: { "Content-Type": "application/json" } },
        ),
      ),
    );
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("renders the API info payload returned by /api/info", async () => {
    renderHomePage();

    await waitFor(() => {
      const block = screen.getByTestId("api-info");
      expect(block.textContent).toContain("Base.Api");
      expect(block.textContent).toContain("9.9.9");
      expect(block.textContent).toContain("Test");
    });
  });
});
