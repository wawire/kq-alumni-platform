import { Users, Briefcase, Heart } from "lucide-react";
import Link from "next/link";

import { Button } from "@/components/ui";

export default function Home() {
  return (
    <main className="bg-gradient-to-br from-gray-50 to-white min-h-screen flex flex-col">
      {/* Hero Section */}
      <section className="flex-1 flex items-center">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12 sm:py-16 md:py-20 w-full">
          <div className="text-center max-w-4xl mx-auto">
            <h1 className="text-3xl sm:text-4xl md:text-5xl lg:text-6xl font-cabrito font-bold text-kq-dark mb-4 sm:mb-6">
              Welcome to the <span className="text-kq-red">Kenya Airways</span>{" "}
              Alumni Association
            </h1>
            <p className="text-base sm:text-lg md:text-xl lg:text-2xl text-gray-600 font-roboto mb-8 sm:mb-10 md:mb-12 leading-relaxed px-4">
              Your journey with Kenya Airways doesn&apos;t end when you leave the
              airline— it&apos;s the beginning of a lifelong connection to a
              family that values you.
            </p>

          <div className="flex flex-col sm:flex-row gap-3 sm:gap-4 justify-center px-4">
            <Link href="/register" className="w-full sm:w-auto">
              <Button variant="primary" size="lg" className="w-full sm:w-auto">
                Register Now
              </Button>
            </Link>
            <a href="mailto:KQ.Alumni@kenya-airways.com" className="w-full sm:w-auto">
              <Button variant="outline" size="lg" className="w-full sm:w-auto">
                Contact Us
              </Button>
            </a>
          </div>
        </div>
        </div>
      </section>

      {/* Features Section */}
      <section className="py-12 sm:py-16 md:py-20">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-6 sm:gap-8">
          <FeatureCard
            icon={<Users className="w-12 h-12 text-kq-red" />}
            title="Network & Connect"
            description="Stay in touch with colleagues and expand your professional network"
          />
          <FeatureCard
            icon={<Briefcase className="w-12 h-12 text-kq-red" />}
            title="Career Growth"
            description="Access opportunities within our global talent pool and mentorship"
          />
          <FeatureCard
            icon={<Heart className="w-12 h-12 text-kq-red" />}
            title="Give Back"
            description="Support CSR initiatives and mentor the next generation"
          />
        </div>
        </div>
      </section>
    </main>
  );
}

interface FeatureCardProps {
  icon: React.ReactNode;
  title: string;
  description: string;
}

function FeatureCard({ icon, title, description }: FeatureCardProps) {
  return (
    <div className="bg-white p-6 sm:p-8 rounded-xl shadow-lg hover:shadow-xl transition-shadow border border-gray-200">
      <div className="mb-4">{icon}</div>
      <h3 className="text-xl sm:text-2xl font-cabrito font-bold text-kq-dark mb-2 sm:mb-3">
        {title}
      </h3>
      <p className="text-sm sm:text-base text-gray-600 font-roboto">{description}</p>
    </div>
  );
}
